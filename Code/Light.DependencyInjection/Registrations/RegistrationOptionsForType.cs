using System;
using System.Collections.Generic;
using System.Linq;
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

        public IRegistrationOptionsForType InstantiateWith(Func<object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<TParameter>(Func<TParameter, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2>(Func<T1, T2, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Type targetType,
                                                      Action<IRegistrationOptionsForType> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));

            var options = new RegistrationOptionsForType(targetType, container.Services.ConstructorSelector, container.Services.IgnoredAbstractionTypes);
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
            selectFieldExpression.MustNotBeNull(nameof(selectFieldExpression));

            AddInstanceInjection(new FieldInjection(selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptionsForType<T> ResolveAllForProperty<TProperty, TItem>(Expression<Func<T, TProperty>> selectPropertyExpression) where TProperty : IEnumerable<TItem>
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            var propertyInfo = selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType);
            InstanceInjections.First(instanceInjection => instanceInjection.MemberName == propertyInfo.Name).DependencyResolver = new ResolveAllDependencyResolver<TItem>();
            return this;
        }

        public IRegistrationOptionsForType<T> InstantiateWith(Func<T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<TParameter>(Func<TParameter, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2>(Func<T1, T2, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        public static DiContainer PerformRegistration(DiContainer container,
                                                      Action<IRegistrationOptionsForType<T>> configureOptions,
                                                      ILifetime lifetime,
                                                      Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));
            lifetime.MustNotBeNull(nameof(lifetime));

            var options = new RegistrationOptionsForType<T>(container.Services.ConstructorSelector, container.Services.IgnoredAbstractionTypes);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateAndAddRegistration(container, lifetime);
            return container;
        }
    }
}