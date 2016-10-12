using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptionsForType : BaseRegistrationOptionsForTypes<IRegistrationOptionsForType>, IRegistrationOptionsForType
    {
        public RegistrationOptionsForType(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes) :
            base(targetType, constructorSelector, ignoredAbstractionTypes) { }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Type targetType,
                                                      Action<IRegistrationOptionsForType> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));

            var options = new RegistrationOptionsForType(targetType, container.ContainerServices.ConstructorSelector, container.ContainerServices.IgnoredAbstractionTypes);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateAndAddRegistration(container, lifetime);
            return container;
        }
    }

    public sealed class RegistrationOptionsForType<T> : BaseRegistrationOptionsForTypes<IRegistrationOptionsForType<T>>, IRegistrationOptionsForType<T>
    {
        public RegistrationOptionsForType(IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
            : base(typeof(T), constructorSelector, ignoredAbstractionTypes) { }

        public IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null)
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            AddInstanceInjection(new PropertyInjection(selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null)
        {
            selectFieldExpression.MustNotBeNull();

            AddInstanceInjection(new FieldInjection(selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate(Func<T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<TParameter>(Func<TParameter, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2>(Func<T1, T2, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3>(Func<T1, T2, T3, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public IRegistrationOptionsForType<T> UseDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance)
        {
            InstantiationInfo = new DelegateInstantiationInfo(createInstance);
            return this;
        }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Action<IRegistrationOptionsForType<T>> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));
            lifetime.MustNotBeNull(nameof(lifetime));

            var options = new RegistrationOptionsForType<T>(container.ContainerServices.ConstructorSelector, container.ContainerServices.IgnoredAbstractionTypes);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateAndAddRegistration(container, lifetime);
            return container;
        }
    }
}