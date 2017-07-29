using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DefaultResolveDelegateFactory : IResolveDelegateFactory
    {
        private static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo()
                                                                                           .GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        private static readonly Type FuncOfObjectType = typeof(Func<object>);

        private readonly IDictionary<Type, IInstantiationExpressionFactory> _instantiationExpressionFactories;

        public DefaultResolveDelegateFactory(IDictionary<Type, IInstantiationExpressionFactory> instantiationExpressionFactories)
        {
            instantiationExpressionFactories.MustNotBeNullOrEmpty(nameof(instantiationExpressionFactories));
            _instantiationExpressionFactories = instantiationExpressionFactories;
        }

        public Func<object> Create(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var resolveExpression = CreateResolveExpressionRecursively(typeKey, container);
            return Expression.Lambda<Func<object>>(resolveExpression).Compile();
        }

        private Expression CreateResolveExpressionRecursively(TypeKey targetTypeKey, DiContainer container)
        {
            var registration = container.GetRegistration(targetTypeKey);

            if (registration.LifeTime.IsCreatingNewInstances == false)
                return Expression.Call(Expression.Constant(registration.LifeTime),
                                       LifetimeResolveInstanceMethod,
                                       Expression.Constant(null, FuncOfObjectType));

            var instantiationDependencies = registration.TypeConstructionInfo.InstantiationInfo.InstantiationDependencies;
            Expression[] parameterExpressions = null;
            if (instantiationDependencies.IsNullOrEmpty() == false)
            {
                parameterExpressions = new Expression[instantiationDependencies.Count];
                for (var i = 0; i < instantiationDependencies.Count; i++)
                {
                    var instantiationDependency = instantiationDependencies[i];
                    parameterExpressions[i] = CreateResolveExpressionRecursively(new TypeKey(instantiationDependency.ParameterInfo.ParameterType, instantiationDependency.TargetRegistrationName), container);
                }
            }

            if (_instantiationExpressionFactories.TryGetValue(registration.TypeConstructionInfo.InstantiationInfo.GetType(), out var instantiationExpressionFactory) == false)
                throw new InvalidOperationException($"There is no instantiationExpressionFactory present for \"{registration.TypeConstructionInfo.InstantiationInfo.GetType()}\". Please check that \"{nameof(DefaultResolveDelegateFactory)}\" is created with all necessary dependencies in \"{nameof(ContainerServices)}\".");
            var createInstanceExpression = instantiationExpressionFactory.Create(registration.TypeConstructionInfo.InstantiationInfo, parameterExpressions);

            // TODO: should special lifetime handling be performed polymorphically?
            if (registration.LifeTime == TransientLifetime.Instance || registration.LifeTime is TransientLifetime)
                return createInstanceExpression;

            var createInstanceDelegate = Expression.Lambda<Func<object>>(createInstanceExpression).Compile();
            var singletonLifetime = registration.LifeTime as SingletonLifetime;
            if (singletonLifetime != null)
            {
                var singleton = singletonLifetime.ResolveInstance(createInstanceDelegate);
                return Expression.Constant(singleton);
            }

            return Expression.Call(Expression.Constant(registration.LifeTime),
                                   LifetimeResolveInstanceMethod,
                                   Expression.Constant(createInstanceDelegate));
        }
    }
}