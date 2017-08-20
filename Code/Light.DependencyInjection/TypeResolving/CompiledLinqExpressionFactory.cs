using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class CompiledLinqExpressionFactory : IResolveDelegateFactory
    {
        private static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo()
                                                                                           .GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        private static readonly MethodInfo ChangeResolvedTypeMethod = KnownTypes.ResolveContextType.GetTypeInfo().GetDeclaredMethod(nameof(ResolveContext.ChangeResolvedType));
        private static readonly ParameterExpression ResolveContextParameterExpression = Expression.Parameter(KnownTypes.ResolveContextType);
        private readonly IReadOnlyDictionary<Type, IInstanceManipulationExpressionFactory> _instanceManipulationExpressionFactories;
        private readonly IReadOnlyDictionary<Type, IInstantiationExpressionFactory> _instantiationExpressionFactories;

        public CompiledLinqExpressionFactory(IReadOnlyDictionary<Type, IInstantiationExpressionFactory> instantiationExpressionFactories,
                                             IReadOnlyDictionary<Type, IInstanceManipulationExpressionFactory> instanceManipulationExpressionFactories)
        {
            instantiationExpressionFactories.MustNotBeNullOrEmpty(nameof(instantiationExpressionFactories));
            instanceManipulationExpressionFactories.MustNotBeNullOrEmpty(nameof(instanceManipulationExpressionFactories));

            _instantiationExpressionFactories = instantiationExpressionFactories;
            _instanceManipulationExpressionFactories = instanceManipulationExpressionFactories;
        }

        public ResolveDelegate Create(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(typeKey, container);
            return resolveExpression.CompileToResolveDelegate(ResolveContextParameterExpression);
        }

        public ResolveDelegate Create(Registration registration, DiContainer container)
        {
            registration.MustNotBeNull(nameof(registration));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(registration.TypeKey, registration, container);
            return resolveExpression.CompileToResolveDelegate(ResolveContextParameterExpression);
        }

        private Expression CreateResolveExpressionRecursively(TypeKey requestedTypeKey, DiContainer container, bool? tryResolveAll = null)
        {
            var resolveInfo = container.GetResolveInfo(requestedTypeKey, tryResolveAll);
            if (resolveInfo is ResolveRegistrationInfo resolveRegistrationInfo)
                return CreateResolveExpressionRecursively(requestedTypeKey, resolveRegistrationInfo.Registration, container);
            if (resolveInfo is ResolveAllInfo resolveAllInfo)
                return CreateResolveAllExpressionRecursively(resolveAllInfo, container);

            throw new InvalidOperationException($"Cannot handle ResolveInfo \"{resolveInfo}\" in this current implementation of \"{nameof(CompiledLinqExpressionFactory)}\"");
        }

        private Expression CreateResolveExpressionRecursively(TypeKey requestedTypeKey, Registration registration, DiContainer container)
        {
            // Check if the lifetime of the registration would need to create a new instance.
            // If not, then we can immediately get the instance of the lifetime without creating a ResolveContext
            if (registration.Lifetime.IsCreatingNewInstances == false)
            {
                var instance = registration.Lifetime.ResolveInstance(null);
                return Expression.Constant(instance, requestedTypeKey.Type);
            }

            // Else we need to create a construction expression that instantiates the target type and performs any instance manipulations
            var resolveExpressionContext = new ResolveExpressionContext(requestedTypeKey, registration, container);
            var constructionExpression = CreateConstructionExpression(resolveExpressionContext);

            // Compile the construction expression to a delegate so that it can be used with lifetimes
            var compiledDelegate = constructionExpression.CompileToResolveDelegate(ResolveContextParameterExpression);

            // TODO: provide an extension point to create expressions for known lifetimes that do not require the compilation of the construction expression
            var targetLifetime = resolveExpressionContext.IsResolvingGenericTypeDefinition ? registration.Lifetime.GetLifetimeInstanceForConstructedGenericType() : registration.Lifetime;
            // If it is a singleton lifetime, then immediately resolve the instance and return it as a constant expression
            if (targetLifetime is SingletonLifetime singletonLifetime)
            {
                // TODO: this code can be optimized because a singleton lifetime might already hold the value; in this case it would be unnecessary to create the compiled delegate
                var singleton = singletonLifetime.ResolveInstance(container.Services.ResolveContextFactory.Create(container).ChangeResolvedType(registration, compiledDelegate));
                return Expression.Constant(singleton, requestedTypeKey.Type);
            }

            // Else create a expression that calls the target lifetime using the compiled delegate
            var resolveContextExpression = Expression.Call(ResolveContextParameterExpression,
                                                           ChangeResolvedTypeMethod,
                                                           Expression.Constant(registration),
                                                           Expression.Constant(compiledDelegate));
            return Expression.Convert(Expression.Call(Expression.Constant(targetLifetime),
                                                      LifetimeResolveInstanceMethod,
                                                      resolveContextExpression),
                                      requestedTypeKey.Type);
        }

        private Expression CreateResolveAllExpressionRecursively(ResolveAllInfo resolveAllInfo, DiContainer container)
        {
            // Create the expression that instantiates the target collection
            var collectionRegistration = container.GetRegistration(resolveAllInfo.CollectionType);
            if (collectionRegistration == null)
                throw new ResolveException($"There is no registration present to resolve collection type \"{resolveAllInfo.CollectionType}\".");
            var createCollectionExpression = CreateConstructionExpression(new ResolveExpressionContext(new TypeKey(resolveAllInfo.CollectionType), collectionRegistration, container));
            
            // Create the expression block that assigns the created collection to a variable, casts this variable to IList<TargetType>, and then resolves all registrations, adding the resulting the expressions
            var blockExpressions = new Expression[resolveAllInfo.Registrations.Count + 3]; // +3 for initial assignment, casting to IList<ItemType>, and return statement
            var variableExpression = Expression.Variable(resolveAllInfo.CollectionType);
            var assignVariableExpression = Expression.Assign(variableExpression, createCollectionExpression);
            blockExpressions[0] = assignVariableExpression; // Assign created collection to variable
            var closedConstructedICollectionType = KnownTypes.ICollectionGenericTypeDefinition.MakeGenericType(resolveAllInfo.ItemType);
            var castedICollectionVariableExpression = Expression.Variable(closedConstructedICollectionType);
            var assignCastedListExpression = Expression.Assign(castedICollectionVariableExpression, Expression.ConvertChecked(variableExpression, closedConstructedICollectionType));
            blockExpressions[1] = assignCastedListExpression; // Assign casted list to variable

            // Resolve all registrations and add them to the collection
            var addMethodInfo = closedConstructedICollectionType.GetRuntimeMethod("Add", new[] { resolveAllInfo.ItemType });
            for (var i = 0; i < resolveAllInfo.Registrations.Count; i++)
            {
                var registration = resolveAllInfo.Registrations[i];
                var itemTypeKey = new TypeKey(resolveAllInfo.ItemType, registration.RegistrationName);
                var resolveRegistrationExpression = CreateResolveExpressionRecursively(itemTypeKey, registration, container);
                blockExpressions[i + 2] = Expression.Call(castedICollectionVariableExpression, addMethodInfo, resolveRegistrationExpression);
            }

            blockExpressions[blockExpressions.Length - 1] = variableExpression; // Return statement

            return Expression.Block(resolveAllInfo.CollectionType, new[] { variableExpression, castedICollectionVariableExpression }, blockExpressions);
        }

        private Expression CreateConstructionExpression(ResolveExpressionContext context)
        {
            // Create the expression that instantiates the target object
            var instantiationInfo = context.Registration.TypeConstructionInfo.InstantiationInfo;
            var parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instantiationInfo.InstantiationDependencies, context);

            // Use the correct factory to create the expression that instantiates the target type
            if (_instantiationExpressionFactories.TryGetValue(instantiationInfo.GetType(), out var instantiationExpressionFactory) == false)
                throw new InvalidOperationException($"There is no instantiationExpressionFactory present for \"{instantiationInfo.GetType()}\". Please check that \"{nameof(CompiledLinqExpressionFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
            var instantiationExpression = instantiationExpressionFactory.Create(instantiationInfo, context, parameterExpressions);

            // If there are no instance manipulations, then simply return the instantiation expression
            var instanceManipulations = context.Registration.TypeConstructionInfo.InstanceManipulations;
            if (instanceManipulations.IsNullOrEmpty())
                return instantiationExpression;

            // Else we need a block that holds the target object in a variable and performs each instance manipulation in one statement
            // The first statement is always the assignment of the instantiationExpression to a variable that holds the instance
            var instanceVariableExpression = Expression.Variable(context.InstanceType);
            var assignVariableExpression = Expression.Assign(instanceVariableExpression, instantiationExpression);
            var blockExpressions = new Expression[instanceManipulations.Count + 2]; // +2 for assignment and return statements
            blockExpressions[0] = assignVariableExpression; // variable assign statement

            // The subsequent statements hold the instance manipulations (e.g. property injection, field injection, calling methods, etc.)
            for (var i = 0; i < instanceManipulations.Count; i++)
            {
                var instanceManipulation = instanceManipulations[i];
                parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instanceManipulation.Dependencies, context);

                if (_instanceManipulationExpressionFactories.TryGetValue(instanceManipulation.GetType(), out var instanceManipulationExpressionFactory) == false)
                    throw new InvalidOperationException($"There is no instanceManipulationExpressionFactory present for \"{instanceManipulation.GetType()}\". Please check that \"{nameof(CompiledLinqExpressionFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
                blockExpressions[i + 1] = instanceManipulationExpressionFactory.Create(instanceManipulation, instanceVariableExpression, context, parameterExpressions);
            }

            blockExpressions[blockExpressions.Length - 1] = instanceVariableExpression; // Return statement

            return Expression.Block(context.InstanceType, new[] { instanceVariableExpression }, blockExpressions);
        }

        private Expression[] ResolveDependenciesRecursivelyIfNecessary(IReadOnlyList<Dependency> dependencies, ResolveExpressionContext context)
        {
            if (dependencies.IsNullOrEmpty())
                return null;

            var resolvedDependencyExpressions = new Expression[dependencies.Count];
            for (var i = 0; i < dependencies.Count; i++)
            {
                var dependency = dependencies[i];
                var dependencyType = dependency.DependencyType;
                if (dependencyType.IsGenericTypeParameter())
                    dependencyType = context.ResolveGenericTypeParameter(dependencyType);
                resolvedDependencyExpressions[i] = CreateResolveExpressionRecursively(new TypeKey(dependencyType, dependency.TargetRegistrationName), context.Container, dependency.ResolveAll);
            }

            return resolvedDependencyExpressions;
        }
    }
}