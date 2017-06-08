using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DefaultStandardizedConstructionFunctionFactory : IStandardizedConstructionFunctionFactory
    {
        public Func<object> Create(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var registration = container.GetRegistration(typeKey);

            if (registration.LifeTime.IsCreatingNewInstances == false)
                return () => registration.LifeTime.ResolveInstance(new ResolveContext());

            if (registration.LifeTime == TransientLifetime.Instance)
            {
                var constructorInstantiationInfo = registration.TypeConstructionInfo.InstantiationInfo as ConstructorInstantiationInfo;
                if (constructorInstantiationInfo != null)
                    return Expression.Lambda<Func<object>>(Expression.New(constructorInstantiationInfo.ConstructorInfo)).Compile();
            }

            throw new NotImplementedException();
        }
    }
}