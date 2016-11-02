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
    /// <summary>
    ///     Represents the options for configuring registrations that are instantiated by the DI container.
    /// </summary>
    public sealed class RegistrationOptionsForType : BaseRegistrationOptionsForType<IRegistrationOptionsForType>, IRegistrationOptionsForType
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationOptionsForType" />
        /// </summary>
        /// <param name="targetType">The target type of these options.</param>
        /// <param name="constructorSelector">The service that selects a constructor from the target type when the client did not specify any.</param>
        /// <param name="ignoredAbstractionTypes">The list of abstraction types that are ignored when abstraction types are mapped to the target type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="targetType" /> is an interface, an abstract class, a generic parameter type, or a open generic type.</exception>
        public RegistrationOptionsForType(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes) :
            base(targetType, constructorSelector, ignoredAbstractionTypes) { }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith(Func<object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<TParameter>(Func<TParameter, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2>(Func<T1, T2, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <summary>
        ///     Creates a <see cref="Registration" /> instance out of these options and registers it with the target container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <param name="targetType">The concrete target type to be registered.</param>
        /// <param name="configureOptions">The delegate that configures an instance of these options.</param>
        /// <param name="lifetime">The lifetime that should be associated with the registration.</param>
        /// <param name="abstractionType">The abstraction type that should be mapped to the target type.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" />, <paramref name="targetType" />, or <paramref name="lifetime" /> is null.</exception>
        public static DependencyInjectionContainer PerformRegistration(DependencyInjectionContainer container,
                                                                       Type targetType,
                                                                       Action<IRegistrationOptionsForType> configureOptions,
                                                                       Lifetime lifetime,
                                                                       Type abstractionType = null)
        {
            container.MustNotBeNull(nameof(container));

            var options = container.Services.CreateRegistrationOptions(targetType);
            if (abstractionType != null)
                options.MapToAbstractions(abstractionType);
            configureOptions?.Invoke(options);

            options.CreateAndAddRegistration(container, lifetime);
            return container;
        }
    }

    /// <summary>
    ///     Represents the options for configuring registrations that are instantiated by the DI container.
    /// </summary>
    /// <typeparam name="T">The concrete target type to be configured.</typeparam>
    public sealed class RegistrationOptionsForType<T> : BaseRegistrationOptionsForType<IRegistrationOptionsForType<T>>, IRegistrationOptionsForType<T>
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationOptionsForType{T}" />
        /// </summary>
        /// <param name="constructorSelector">The service that selects a constructor from the target type when the client did not specify any.</param>
        /// <param name="ignoredAbstractionTypes">The list of abstraction types that are ignored when abstraction types are mapped to the target type.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the target type is an interface, an abstract class, a generic parameter type, or a open generic type.</exception>
        public RegistrationOptionsForType(IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
            : base(typeof(T), constructorSelector, ignoredAbstractionTypes) { }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string targetRegistrationName = null)
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            AddInstanceInjection(new PropertyInjection(TargetType, selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType), targetRegistrationName));
            return this;
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string targetRegistrationName = null)
        {
            selectFieldExpression.MustNotBeNull(nameof(selectFieldExpression));

            AddInstanceInjection(new FieldInjection(TargetType, selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType), targetRegistrationName));
            return this;
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> ResolveAllForProperty<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression)
        {
            var propertyInfo = selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType);
            var itemType = propertyInfo.PropertyType.GetItemTypeOfEnumerable();
            InstanceInjections.First(instanceInjection => instanceInjection.MemberName == propertyInfo.Name).DependencyResolver = ResolveAll.Create(itemType);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> ResolveAllForField<TField>(Expression<Func<T, TField>> selectFieldExpression)
        {
            var fieldInfo = selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType);
            var itemType = fieldInfo.FieldType.GetItemTypeOfEnumerable();
            InstanceInjections.First(instanceInjection => instanceInjection.MemberName == fieldInfo.Name).DependencyResolver = ResolveAll.Create(itemType);
            return this;
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith(Func<T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<TParameter>(Func<TParameter, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2>(Func<T1, T2, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <inheritdoc />
        public IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance)
        {
            return SetDelegateInstantiationInfo(createInstance);
        }

        /// <summary>
        ///     Creates a <see cref="Registration" /> instance out of these options and registers it with the target container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <param name="configureOptions">The delegate that configures an instance of these options.</param>
        /// <param name="lifetime">The lifetime that should be associated with the registration.</param>
        /// <param name="abstractionType">The abstraction type that should be mapped to the target type.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="lifetime" /> is null.</exception>
        public static DependencyInjectionContainer PerformRegistration(DependencyInjectionContainer container,
                                                                       Action<IRegistrationOptionsForType<T>> configureOptions,
                                                                       Lifetime lifetime,
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