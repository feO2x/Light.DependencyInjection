using System;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DefaultStandardizedConstructionFunctionFactory : IStandardizedConstructionFunctionFactory
    {
        private static readonly MethodInfo LifetimeResolveInstanceMethod = typeof(Lifetime).GetTypeInfo()
                                                                                           .GetDeclaredMethod(nameof(Lifetime.ResolveInstance));

        public Func<object> Create(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var registration = container.GetRegistration(typeKey);

            if (registration.LifeTime.IsCreatingNewInstances == false)
                return () => registration.LifeTime.ResolveInstance(null);

            var createInstance = BuildCreateInstanceExpression(registration);

            if (registration.LifeTime == TransientLifetime.Instance)
            {
                return createInstance;
            }

            var singletonLifetime = registration.LifeTime as SingletonLifetime;
            if (singletonLifetime != null)
            {
                var singletonInstance = singletonLifetime.ResolveInstance(createInstance);
                return Expression.Lambda<Func<object>>(Expression.Constant(singletonInstance)).Compile();
            }

            return Expression.Lambda<Func<object>>(Expression.Call(Expression.Constant(registration.LifeTime),
                                                                   LifetimeResolveInstanceMethod,
                                                                   Expression.Constant(createInstance)))
                             .Compile();
        }

        private static Func<object> BuildCreateInstanceExpression(Registration registration)
        {
            var constructorInstantiationInfo = registration.TypeConstructionInfo.InstantiationInfo as ConstructorInstantiationInfo;
            if (constructorInstantiationInfo != null)
                return Expression.Lambda<Func<object>>(Expression.New(constructorInstantiationInfo.ConstructorInfo)).Compile();

            throw new NotImplementedException();
        }
    }
}