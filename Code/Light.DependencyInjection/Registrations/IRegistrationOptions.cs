using System;
using System.Collections.Generic;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;

namespace Light.DependencyInjection.Registrations
{
    public interface ICommonRegistrationOptions<out TOptions> where TOptions : ICommonRegistrationOptions<TOptions>
    {
        TOptions UseRegistrationName(string registrationName);

        TOptions UseTypeNameAsRegistrationName();

        TOptions UseFullTypeNameAsRegistrationName();

        TOptions DisableIDisposableTracking();

        TOptions MapToAbstractions(params Type[] abstractionTypes);

        TOptions MapToAbstractions(IEnumerable<Type> abstractionTypes);

        TOptions MapToAllImplementedInterfaces();

        TOptions MapToBaseClass();
    }

    public interface IExternalInstanceOptions : ICommonRegistrationOptions<IExternalInstanceOptions> { }

    public interface ICreateInstanceOptions<out TOptions> : ICommonRegistrationOptions<TOptions> where TOptions : ICreateInstanceOptions<TOptions>
    {
        TOptions UseConstructor(ConstructorInfo constructorInfo);

        TOptions UseDefaultConstructor();

        TOptions UseConstructorWithParameters(params Type[] parameterTypes);

        TOptions UseConstructorWithParameter<T>();

        TOptions UseConstructorWithParameter<T1, T2>();
        TOptions UseConstructorWithParameter<T1, T2, T3>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        TOptions UseConstructorWithParameter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();

        TOptions InstantiateVia(MethodInfo methodInfo);

        TOptions AddPropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = "");
        TOptions AddPropertyInjection(string propertyName, string targetRegistrationName = "");

        TOptions AddFieldInjection(FieldInfo fieldInfo, string targetRegistrationName = "");
        TOptions AddFieldInjection(string fieldName, string targetRegistrationName = "");

        TOptions UseLifetime(Lifetime lifetime);
    }

    public interface IRegistrationOptions : ICreateInstanceOptions<IRegistrationOptions>
    {
        IRegistrationOptions InstantiateVia(Func<object> createInstance);
        IRegistrationOptions InstantiateVia<T>(Func<T, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2>(Func<T1, T2, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance);
        IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance);
    }


    public interface IRegistrationOptions<in T> : ICreateInstanceOptions<IRegistrationOptions<T>>
    {
        IRegistrationOptions<T> InstantiateVia(Func<T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1>(Func<T1, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2>(Func<T1, T2, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance);
        IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance);
    }
}