using System;
using System.Collections.Generic;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IRegistrationOptions
    {
        IRegistrationOptions WithRegistrationName(string registrationName);
        IRegistrationOptions UseConstructor(ConstructorInfo constructorInfo);
        IRegistrationOptions UseDefaultConstructor();
        IRegistrationOptions UseConstructorWithParameters(params Type[] parameterTypes);
        IRegistrationOptions UseConstructorWithParameter<TParameter>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        IRegistrationOptions MapTypeToAbstractions(params Type[] abstractionTypes);
        IRegistrationOptions MapTypeToAbstractions(IEnumerable<Type> abstractionTypes);
        IRegistrationOptions MapTypeToAllImplementedInterfaces();
    }
}