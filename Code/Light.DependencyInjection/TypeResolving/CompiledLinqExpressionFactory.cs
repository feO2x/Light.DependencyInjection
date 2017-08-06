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

        private Expression CreateResolveExpressionRecursively(TypeKey targetTypeKey, DiContainer container)
        {
            // First get the registration that maps to the target type key
            var registration = container.GetRegistration(targetTypeKey);

            // Check if the lifetime of the registration would need to create a new instance.
            // If not, then we can immediately get the instance of the lifetime without creating a ResolveContext
            if (registration.Lifetime.IsCreatingNewInstances == false)
            {
                var instance = registration.Lifetime.ResolveInstance(null);
                return Expression.Constant(instance, targetTypeKey.Type);
            }

            // Else we need to create a construction expression that instantiates the target type and performs any instance manipulations
            var constructionExpression = CreateConstructionExpression(targetTypeKey.Type, registration, container);

            // TODO: provide an extension point to create expressions for known lifetimes that do not require the compilation of the construction expression
            // Compile the construction expression to a delegate so that it can be used with lifetimes
            var compiledDelegate = constructionExpression.CompileToResolveDelegate(ResolveContextParameterExpression);

            // If it is a singleton lifetime, then immediately resolve the instance and return it as a constant expression
            if (registration.Lifetime is SingletonLifetime singletonLifetime)
            {
                var singleton = singletonLifetime.ResolveInstance(container.Services.ResolveContextFactory.Create(container).ChangeResolvedType(registration, compiledDelegate));
                return Expression.Constant(singleton, targetTypeKey.Type);
            }

            // Else create a expression that calls the target lifetime using the compiled delegate
            var resolveContextExpression = Expression.Call(ResolveContextParameterExpression,
                                                           ChangeResolvedTypeMethod,
                                                           Expression.Constant(registration),
                                                           Expression.Constant(compiledDelegate));
            return Expression.Convert(Expression.Call(Expression.Constant(registration.Lifetime),
                                                      LifetimeResolveInstanceMethod,
                                                      resolveContextExpression),
                                      targetTypeKey.Type);
        }

        private Expression CreateConstructionExpression(Type requestedType, Registration registration, DiContainer container)
        {
            // Create the expression that instantiates the target object
            var instantiationDependencies = registration.TypeConstructionInfo.InstantiationInfo.InstantiationDependencies;
            var parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instantiationDependencies, container);

            // Use the correct factory to create the expression that instantiates the target type
            if (_instantiationExpressionFactories.TryGetValue(registration.TypeConstructionInfo.InstantiationInfo.GetType(), out var instantiationExpressionFactory) == false)
                throw new InvalidOperationException($"There is no instantiationExpressionFactory present for \"{registration.TypeConstructionInfo.InstantiationInfo.GetType()}\". Please check that \"{nameof(CompiledLinqExpressionFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
            var instantiationExpression = instantiationExpressionFactory.Create(registration.TypeConstructionInfo.InstantiationInfo, requestedType, parameterExpressions);

            // If there are no instance manipulations, then simply return the instantiation expression
            if (registration.TypeConstructionInfo.InstanceManipulations.IsNullOrEmpty())
                return instantiationExpression;

            // Else we need a block that holds the target object in a variable and performs each instance manipulation in one statement
            // The first statement is always the assignment of the instantiationExpression to a variable that holds the instance
            var instanceVariableExpression = Expression.Variable(registration.TargetType);
            var assignVariableExpression = Expression.Assign(instanceVariableExpression, instantiationExpression);
            var blockExpressions = new Expression[registration.TypeConstructionInfo.InstanceManipulations.Count + 2]; // +2 for assignment and return statements
            blockExpressions[0] = assignVariableExpression; // variable assign statement

            // The subsequent statements hold the instance manipulations (e.g. property injection, field injection, calling methods, etc.)
            for (var i = 0; i < registration.TypeConstructionInfo.InstanceManipulations.Count; i++)
            {
                var instanceManipulation = registration.TypeConstructionInfo.InstanceManipulations[i];
                parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instanceManipulation.Dependencies, container);

                if (_instanceManipulationExpressionFactories.TryGetValue(instanceManipulation.GetType(), out var instanceManipulationExpressionFactory) == false)
                    throw new InvalidOperationException($"There is no instanceManipulationExpressionFactory present for \"{instanceManipulation.GetType()}\". Please check that \"{nameof(CompiledLinqExpressionFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
                blockExpressions[i + 1] = instanceManipulationExpressionFactory.Create(instanceManipulation, instanceVariableExpression, parameterExpressions);
            }

            blockExpressions[blockExpressions.Length - 1] = instanceVariableExpression; // Return statement

            return Expression.Block(registration.TargetType, new[] { instanceVariableExpression }, blockExpressions);
        }

        private Expression[] ResolveDependenciesRecursivelyIfNecessary(IReadOnlyList<Dependency> dependencies, DiContainer container)
        {
            if (dependencies.IsNullOrEmpty())
                return null;

            var resolvedDependencyExpressions = new Expression[dependencies.Count];
            for (var i = 0; i < dependencies.Count; i++)
            {
                var dependency = dependencies[i];
                resolvedDependencyExpressions[i] = CreateResolveExpressionRecursively(new TypeKey(dependency.DependencyType, dependency.TargetRegistrationName), container);
            }

            return resolvedDependencyExpressions;
        }
    }
}