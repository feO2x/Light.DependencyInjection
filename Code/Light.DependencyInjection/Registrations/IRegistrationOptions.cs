using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IBaseRegistrationOptionsForExternalInstance<out TConcreteOptions>
        where TConcreteOptions : class, IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
    {
        TConcreteOptions UseRegistrationName(string registrationName);
        TConcreteOptions UseTypeNameAsRegistrationName();
        TConcreteOptions UseFullTypeNameAsRegistrationName();
        TConcreteOptions DisableIDisposableTrackingForThisType();
        TConcreteOptions MapToAbstractions(params Type[] abstractionTypes);
        TConcreteOptions MapToAbstractions(IEnumerable<Type> abstractionTypes);
        TConcreteOptions MapToAllImplementedInterfaces();
    }

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
        TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null);
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter<TParameter>();
    }

    public interface IRegistrationOptionsForType : IBaseRegistrationOptionsForType<IRegistrationOptionsForType>
    {
        IRegistrationOptionsForType InstantiateVia(Func<object> createInstance);
        IRegistrationOptionsForType InstantiateVia<TParameter>(Func<TParameter, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2>(Func<T1, T2, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance);
        IRegistrationOptionsForType InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance);
    }

    public interface IRegistrationOptionsForType<T> : IBaseRegistrationOptionsForType<IRegistrationOptionsForType<T>>
    {
        IRegistrationOptionsForType<T> InstantiateVia(Func<T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<TParameter>(Func<TParameter, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2>(Func<T1, T2, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance);
        IRegistrationOptionsForType<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance);
              
        IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
    }
}