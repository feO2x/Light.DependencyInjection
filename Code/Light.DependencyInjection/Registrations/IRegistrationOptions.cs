using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the abstraction for configuring registrations that return an externally created instance (i.e. the DI container does not create instances of the corresponding type).
    /// </summary>
    /// <typeparam name="TConcreteOptions">The options type that is returned by the fluent API.</typeparam>
    public interface IBaseRegistrationOptionsForExternalInstance<out TConcreteOptions>
        where TConcreteOptions : class, IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
    {
        /// <summary>
        ///     Associates the given name with the registration.
        /// </summary>
        TConcreteOptions UseRegistrationName(string registrationName);

        /// <summary>
        ///     Uses the name (not full name) of the concrete target type as the registration name.
        /// </summary>
        TConcreteOptions UseTypeNameAsRegistrationName();

        /// <summary>
        ///     Uses the full type name of the concrete target type as the registration name.
        /// </summary>
        TConcreteOptions UseFullTypeNameAsRegistrationName();

        /// <summary>
        ///     The DI container will not track disposable instances if you set this option. Only applies when the concrete type implements <see cref="IDisposable" />.
        /// </summary>
        TConcreteOptions DisableIDisposableTrackingForThisType();

        /// <summary>
        ///     Maps the given abstractions to the concrete type.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the concrete type does not implement or derive from any of the given abstraction types.</exception>
        TConcreteOptions MapToAbstractions(params Type[] abstractionTypes);

        /// <summary>
        ///     Maps the given abstractions to the concrete type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="abstractionTypes" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the concrete type does not implement or derive from any of the given abstraction types.</exception>
        TConcreteOptions MapToAbstractions(IEnumerable<Type> abstractionTypes);

        /// <summary>
        ///     Maps the concrete type to all interfaces that it implements. The interfaces of <see cref="ContainerServices.IgnoredAbstractionTypes" /> will not be mapped.
        /// </summary>
        TConcreteOptions MapToAllImplementedInterfaces();
    }

    /// <summary>
    ///     Represents the abstraction for configuring registrations that return an externally created instance (i.e. the DI container does not create instances of the corresponding type).
    ///     Is a variant of <see cref="IBaseRegistrationOptionsForExternalInstance{TConcreteOptions}" /> without the generic parameter for increased readability.
    /// </summary>
    public interface IRegistrationOptionsForExternalInstance : IBaseRegistrationOptionsForExternalInstance<IRegistrationOptionsForExternalInstance> { }

    public interface IBaseRegistrationOptionsForType<out TConcreteOptions> : IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
        where TConcreteOptions : class, IBaseRegistrationOptionsForType<TConcreteOptions>
    {
        TConcreteOptions UseConstructor(ConstructorInfo constructorInfo);
        TConcreteOptions UseDefaultConstructor();
        TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes);
        TConcreteOptions UseConstructorWithParameter<TParameter>();
        TConcreteOptions UseConstructorWithParameters<T1, T2>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = null);
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string targetRegistrationName = null);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter<TParameter>();
        TConcreteOptions ResolveAllForInstantiationParameter<TParameter>();
        TConcreteOptions ResolveAllForInstantiationParameter(string parameterName);
    }

    public interface IRegistrationOptionsForType : IBaseRegistrationOptionsForType<IRegistrationOptionsForType>
    {
        IRegistrationOptionsForType InstantiateWith(Func<object> createInstance);
        IRegistrationOptionsForType InstantiateWith<TParameter>(Func<TParameter, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2>(Func<T1, T2, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance);
        IRegistrationOptionsForType InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance);
    }

    public interface IRegistrationOptionsForType<T> : IBaseRegistrationOptionsForType<IRegistrationOptionsForType<T>>
    {
        IRegistrationOptionsForType<T> InstantiateWith(Func<T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<TParameter>(Func<TParameter, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2>(Func<T1, T2, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3>(Func<T1, T2, T3, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateWith<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance);
        IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForType<T> ResolveAllForProperty<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression);
        IRegistrationOptionsForType<T> ResolveAllForField<TField>(Expression<Func<T, TField>> selectFieldExpression);
    }
}