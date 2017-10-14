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

        public ResolveDelegate Create(TypeKey typeKey, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(typeKey, container, dependencyOverrides);
            return resolveExpression.CompileToResolveDelegate(ResolveContextParameterExpression);
        }

        public ResolveDelegate Create(Registration registration, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container)
        {
            registration.MustNotBeNull(nameof(registration));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(registration.TypeKey, registration, dependencyOverrides, container);
            return resolveExpression.CompileToResolveDelegate(ResolveContextParameterExpression);
        }

        private Expression CreateResolveExpressionRecursively(TypeKey requestedTypeKey, DependencyInjectionContainer container, DependencyOverrides dependencyOverrides, bool? tryResolveAll = null)
        {
            // Check if the instance is overridden
            if (dependencyOverrides?.HasOverriddenInstance(requestedTypeKey) == true)
            {
                return Expression.Convert(Expression.Call(Expression.Call(ResolveContextParameterExpression, GetDependencyOverridesProperty),
                                                          GetOverriddenInstanceMethod,
                                                          Expression.Constant(requestedTypeKey)),
                                          requestedTypeKey.Type);
            }

            // Else ask the container for a ResolveInfo to identify how the returned expression should be composed
            var resolveInfo = container.GetResolveInfo(requestedTypeKey, tryResolveAll);
            if (resolveInfo is ResolveRegistrationInfo resolveRegistrationInfo)
                return CreateResolveExpressionRecursively(requestedTypeKey, resolveRegistrationInfo.Registration, dependencyOverrides, container);
            if (resolveInfo is ResolveAllInfo resolveAllInfo)
                return CreateResolveAllExpressionRecursively(resolveAllInfo, dependencyOverrides, container);

            throw new InvalidOperationException($"Cannot handle ResolveInfo \"{resolveInfo}\" in this current implementation of \"{nameof(CompiledLinqExpressionFactory)}\"");
        }

        private Expression CreateResolveExpressionRecursively(TypeKey requestedTypeKey, Registration registration, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container)
        {
            // Check if the lifetime of the registration would need to create a new instance.
            // If not, then we can do not need to create a construction expression
            Expression resolveContextExpression;
            if (registration.Lifetime.IsCreatingNewInstances == false)
            {
                // If the lifetime can be resolved during compilation, then create a resolve context and immediately
                // call ResolveInstance to compile a constant reference to the instance into the resolve method
                if (registration.Lifetime.CanBeResolvedDuringCompilation)
                {
                    var instance = registration.Lifetime.ResolveInstance(container.Services.ResolveContextFactory.Create(container).ChangeRegistration(registration));
                    return Expression.Constant(instance, requestedTypeKey.Type);
                }

                // Else prepare the resolve context parameter with the target registration and call the ResolveInstance dynamically
                resolveContextExpression = Expression.Call(ResolveContextParameterExpression,
                                                           ChangeRegistrationMethod,
                                                           Expression.Constant(registration));
                return Expression.Convert(Expression.Call(Expression.Constant(registration.Lifetime),
                                                          LifetimeResolveInstanceMethod,
                                                          resolveContextExpression),
                                          requestedTypeKey.Type);
            }

            // Else we need to create a construction expression that instantiates the target type and performs any instance manipulations
            var resolveExpressionContext = new ResolveExpressionContext(requestedTypeKey, registration, dependencyOverrides, container, ResolveContextParameterExpression);
            var constructionExpression = CreateConstructionExpression(resolveExpressionContext);

            // Check if the lifetime implements IOptimizeLifetimeExpression to avoid compiling the construction expression to a delegate
            if (registration.Lifetime is IOptimizeLifetimeExpression optimizeLifetimeExpression)
            {
                return optimizeLifetimeExpression.Optimize(constructionExpression, ResolveContextParameterExpression, resolveExpressionContext);
            }

            // Compile the construction expression to a delegate so that it can be injected into the ResolveContext
            var compiledDelegate = constructionExpression.CompileToResolveDelegate(ResolveContextParameterExpression);

            var targetLifetime = resolveExpressionContext.IsResolvingGenericTypeDefinition ? registration.Lifetime.GetLifetimeInstanceForConstructedGenericType() : registration.Lifetime;

            // If the lifetime can be resolved during compilation, then create a resolve context and immediately
            // call ResolveInstance to compile a constant reference to the instance into the resolve method
            if (targetLifetime.CanBeResolvedDuringCompilation)
            {
                var instance = targetLifetime.ResolveInstance(container.Services.ResolveContextFactory.Create(container).ChangeResolvedType(registration, compiledDelegate));
                return Expression.Constant(instance);
            }

            // Else create a expression that calls the target lifetime using the compiled delegate
            resolveContextExpression = Expression.Call(ResolveContextParameterExpression,
                                                       ChangeResolvedTypeMethod,
                                                       Expression.Constant(registration),
                                                       Expression.Constant(compiledDelegate));
            return Expression.Convert(Expression.Call(Expression.Constant(targetLifetime),
                                                      LifetimeResolveInstanceMethod,
                                                      resolveContextExpression),
                                      requestedTypeKey.Type);
        }

        private Expression CreateResolveAllExpressionRecursively(ResolveAllInfo resolveAllInfo, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container)
        {
            // Create the expression that instantiates the target collection
            var collectionRegistration = container.GetRegistration(resolveAllInfo.CollectionType);
            if (collectionRegistration == null)
                throw new ResolveException($"There is no registration present to resolve collection type \"{resolveAllInfo.CollectionType}\".");
            var createCollectionExpression = CreateConstructionExpression(new ResolveExpressionContext(new TypeKey(resolveAllInfo.CollectionType), collectionRegistration, dependencyOverrides, container, ResolveContextParameterExpression));

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
                var resolveRegistrationExpression = CreateResolveExpressionRecursively(itemTypeKey, registration, dependencyOverrides, container);
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
            for (var i = 0; i < resolvedDependencyExpressions.Length; i++)
            {
                var dependency = dependencies[i];
                var dependencyType = dependency.TargetType;
                dependencyType = context.ResolveGenericTypeParameterIfNecessary(dependencyType);
                dependencyType = context.ResolveOpenConstructedGenericTypeIfNecessary(dependencyType);

                // Check if the dependency is overridden - if yes, then resolve it via the ResolveContext parameter
                if (context.DependencyOverrides?.HasDependency(dependency) == true)
                {
                    resolvedDependencyExpressions[i] = Expression.Convert(Expression.Call(Expression.Call(ResolveContextParameterExpression, GetDependencyOverridesProperty),
                                                                                          GetDependencyInstanceMethod,
                                                                                          Expression.Constant(dependency)),
                                                                          dependency.TargetType);
                    continue;
                }

                resolvedDependencyExpressions[i] = CreateResolveExpressionRecursively(new TypeKey(dependencyType, dependency.TargetRegistrationName),
                                                                                      context.Container,
                                                                                      context.DependencyOverrides,
                                                                                      dependency.ResolveAll);
            }

            return resolvedDependencyExpressions;
        }
    }
}