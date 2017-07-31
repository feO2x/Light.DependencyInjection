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
    public sealed class DefaultResolveDelegateFactory : IResolveDelegateFactory
    {
        private static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo()
                                                                                           .GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        private static readonly ConstructorInfo ResolveContextConstructor = typeof(ResolveContext).GetTypeInfo()
                                                                                                  .DeclaredConstructors
                                                                                                  .FindConstructorWithArgumentTypes(typeof(Func<object>), typeof(ContainerScope), typeof(Registration));

        private readonly IReadOnlyDictionary<Type, IInstanceManipulationExpressionFactory> _instanceManipulationExpressionFactories;

        private readonly IReadOnlyDictionary<Type, IInstantiationExpressionFactory> _instantiationExpressionFactories;

        public DefaultResolveDelegateFactory(IReadOnlyDictionary<Type, IInstantiationExpressionFactory> instantiationExpressionFactories,
                                             IReadOnlyDictionary<Type, IInstanceManipulationExpressionFactory> instanceManipulationExpressionFactories)
        {
            instantiationExpressionFactories.MustNotBeNullOrEmpty(nameof(instantiationExpressionFactories));
            instanceManipulationExpressionFactories.MustNotBeNullOrEmpty(nameof(instanceManipulationExpressionFactories));

            _instantiationExpressionFactories = instantiationExpressionFactories;
            _instanceManipulationExpressionFactories = instanceManipulationExpressionFactories;
        }

        public Func<object> Create(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(typeKey, container);
            return resolveExpression.CompileToFuncOfObject();
        }

        private Expression CreateResolveExpressionRecursively(TypeKey targetTypeKey, DiContainer container)
        {
            var registration = container.GetRegistration(targetTypeKey);

            if (registration.LifeTime.IsCreatingNewInstances == false)
            {
                var instance = registration.LifeTime.ResolveInstance(new ResolveContext());
                return Expression.Constant(instance, registration.TargetType);
            }

            var constructionExpression = CreateConstructionExpression(registration, container);

            // TODO: provide an extension point to create expressions for known lifetime types that do not require the compilation of the construction expression
            var createInstanceDelegate = constructionExpression.CompileToFuncOfObject();
            var singletonLifetime = registration.LifeTime as SingletonLifetime;
            if (singletonLifetime != null)
            {
                var singleton = singletonLifetime.ResolveInstance(new ResolveContext(createInstanceDelegate, container.ContainerScope, registration));
                return Expression.Constant(singleton, registration.TargetType);
            }

            var resolveContextExpression = Expression.New(ResolveContextConstructor,
                                                          Expression.Constant(createInstanceDelegate),
                                                          Expression.Constant(container.ContainerScope),
                                                          Expression.Constant(registration));
            return Expression.Convert(Expression.Call(Expression.Constant(registration.LifeTime),
                                                      LifetimeResolveInstanceMethod,
                                                      resolveContextExpression),
                                      registration.TargetType);
        }

        private Expression CreateConstructionExpression(Registration registration, DiContainer container)
        {
            // Create the expression that instantiates the target object
            var instantiationDependencies = registration.TypeConstructionInfo.InstantiationInfo.InstantiationDependencies;
            var parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instantiationDependencies, container);

            // Use the correct factory to create the expression that instantiates the target type
            if (_instantiationExpressionFactories.TryGetValue(registration.TypeConstructionInfo.InstantiationInfo.GetType(), out var instantiationExpressionFactory) == false)
                throw new InvalidOperationException($"There is no instantiationExpressionFactory present for \"{registration.TypeConstructionInfo.InstantiationInfo.GetType()}\". Please check that \"{nameof(DefaultResolveDelegateFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
            var instantiationExpression = instantiationExpressionFactory.Create(registration.TypeConstructionInfo.InstantiationInfo, parameterExpressions);

            // If there are no instance manipulations, then simply return the instantiation expression
            if (registration.TypeConstructionInfo.InstanceManipulations.IsNullOrEmpty())
                return instantiationExpression;

            // Else we need a block that holds the target object in a variable and performs each instance manipulation in one statement
            // The first statement is always the assignment of the instantiationExpression to a variable that holds the instance
            var instanceVariableExpression = Expression.Variable(registration.TargetType);
            var assignVariableExpression = Expression.Assign(instanceVariableExpression, instantiationExpression);
            var blockExpressions = new Expression[registration.TypeConstructionInfo.InstanceManipulations.Count + 2];   // +2 for assignment and return statements
            blockExpressions[0] = assignVariableExpression; // variable assign statement

            // The subsequent statements hold the instance manipulations (e.g. property injection, field injection, calling methods, etc.)
            for (var i = 0; i < registration.TypeConstructionInfo.InstanceManipulations.Count; i++)
            {
                var instanceManipulation = registration.TypeConstructionInfo.InstanceManipulations[i];
                parameterExpressions = ResolveDependenciesRecursivelyIfNecessary(instanceManipulation.Dependencies, container);

                if (_instanceManipulationExpressionFactories.TryGetValue(instanceManipulation.GetType(), out var instanceManipulationExpressionFactory) == false)
                    throw new InvalidOperationException($"There is no instanceManipulationExpressionFactory present for \"{instanceManipulation.GetType()}\". Please check that \"{nameof(DefaultResolveDelegateFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
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