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

    /// <summary>
    ///     Represents the abstraction for configuring registrations that are instantiated by the DI container.
    /// </summary>
    /// <typeparam name="TConcreteOptions">The options type that is returned by the fluent API.</typeparam>
    public interface IBaseRegistrationOptionsForType<out TConcreteOptions> : IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
        where TConcreteOptions : class, IBaseRegistrationOptionsForType<TConcreteOptions>
    {
        /// <summary>
        ///     Configures the specified constructor to be called when the target type is instantiated by the DI container.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the specified <paramref name="constructorInfo" /> does not describe a constructor of the target concrete type.</exception>
        TConcreteOptions UseConstructor(ConstructorInfo constructorInfo);

        /// <summary>
        ///     Configures the registration to use the default constructor of the target type.
        /// </summary>
        TConcreteOptions UseDefaultConstructor();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <param name="parameterTypes">The parameter types of the target constructor.</param>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes);

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameter.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified type could not be found.</exception>
        TConcreteOptions UseConstructorWithParameter<TParameter>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9>();

        /// <summary>
        ///     Configures the registration to use the constructor of the target type with the specified parameters.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when the target constructor with the specified types could not be found.</exception>
        TConcreteOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();

        /// <summary>
        ///     Configures the registration to use the specified static method to instantiate the target type.
        /// </summary>
        /// <param name="staticFactoryMethodInfo">The info describing a static method returning the target type.</param>
        TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);

        /// <summary>
        ///     Configures the registration to perform the specified property injection after an instance of the target type was created.
        /// </summary>
        /// <param name="propertyInfo">The info describing the target proprety to be injected.</param>
        /// <param name="targetRegistrationName">The registration name used to resolve the child value for the property (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the specified <paramref name="propertyInfo" /> does not describe a public settable instance property of the target type.</exception>
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = null);

        /// <summary>
        ///     Configures the registration to perform the specified field injection after an instance of the target type was created.
        /// </summary>
        /// <param name="fieldInfo">The info describing the target field to be injected.</param>
        /// <param name="targetRegistrationName">The registration name used to resolve the child value for the field (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the specified <paramref name="fieldInfo" /> does not describe a public settable instance field of the target type.</exception>
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string targetRegistrationName = null);

        /// <summary>
        ///     Configures the registration to resolve the instantiation parameter with the specified <paramref name="parameterName" /> using a specific registration name.
        /// </summary>
        /// <param name="parameterName">The parameter name of the instantiation method that should be configured.</param>
        /// <exception cref="TypeRegistrationException">Thrown when there is no parameter with the specified name.</exception>
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);

        /// <summary>
        ///     Configures the registration to resolve the instantiation parameter with the specified type using a specific registration name.
        ///     The type must be unique in the parameter list, otherwise a <see cref="TypeRegistrationException" /> is thrown.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter. This type must be unique in the parameter list.</typeparam>
        /// <exception cref="TypeRegistrationException">Thrown when there is no parameter with the specified type.</exception>
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter<TParameter>();

        /// <summary>
        ///     Configures the registration to resolve all instances for the instatiation parameter with the specified <paramref name="parameterName" />.
        /// </summary>
        /// <param name="parameterName">The parameter name of the instantiation method that should be configured.</param>
        /// <exception cref="TypeRegistrationException">Thrown when there is no parameter with the specified name.</exception>
        TConcreteOptions ResolveAllForInstantiationParameter(string parameterName);

        /// <summary>
        ///     Configures the registration to resolve all instances for the instatiation parameter with the specified type.
        ///     The type must be unique in the parameter list, otherwise a <see cref="TypeRegistrationException" /> is thrown.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter. This type must be unique in the parameter list.</typeparam>
        /// <exception cref="TypeRegistrationException">Thrown when there is no parameter with the specified type.</exception>
        TConcreteOptions ResolveAllForInstantiationParameter<TParameter>();
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