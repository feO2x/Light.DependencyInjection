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

        private readonly IReadOnlyDictionary<Type, IInstantiationExpressionFactory> _instantiationExpressionFactories;

        public DefaultResolveDelegateFactory(IReadOnlyDictionary<Type, IInstantiationExpressionFactory> instantiationExpressionFactories)
        {
            instantiationExpressionFactories.MustNotBeNullOrEmpty(nameof(instantiationExpressionFactories));
            _instantiationExpressionFactories = instantiationExpressionFactories;
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

            var instantiationDependencies = registration.TypeConstructionInfo.InstantiationInfo.InstantiationDependencies;
            Expression[] parameterExpressions = null;
            if (instantiationDependencies.IsNullOrEmpty() == false)
            {
                parameterExpressions = new Expression[instantiationDependencies.Count];
                for (var i = 0; i < instantiationDependencies.Count; i++)
                {
                    var instantiationDependency = instantiationDependencies[i];
                    parameterExpressions[i] = CreateResolveExpressionRecursively(new TypeKey(instantiationDependency.DependencyType, instantiationDependency.TargetRegistrationName), container);
                }
            }

            if (_instantiationExpressionFactories.TryGetValue(registration.TypeConstructionInfo.InstantiationInfo.GetType(), out var instantiationExpressionFactory) == false)
                throw new InvalidOperationException($"There is no instantiationExpressionFactory present for \"{registration.TypeConstructionInfo.InstantiationInfo.GetType()}\". Please check that \"{nameof(DefaultResolveDelegateFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
            var createInstanceExpression = instantiationExpressionFactory.Create(registration.TypeConstructionInfo.InstantiationInfo, parameterExpressions);

            // TODO: provide an extension point to create expressions for known lifetime types
            var createInstanceDelegate = createInstanceExpression.CompileToFuncOfObject();
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
    }
}