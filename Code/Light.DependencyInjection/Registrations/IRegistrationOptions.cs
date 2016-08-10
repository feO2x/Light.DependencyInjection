using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IRegistrationOptions<T>
    {
        IRegistrationOptions<T> WithRegistrationName(string registrationName);
        IRegistrationOptions<T> UseConstructor(ConstructorInfo constructorInfo);
        IRegistrationOptions<T> UseDefaultConstructor();
        IRegistrationOptions<T> UseConstructorWithParameters(params Type[] parameterTypes);
        IRegistrationOptions<T> UseConstructorWithParameter<TParameter>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        IRegistrationOptions<T> MapTypeToAbstractions(params Type[] abstractionTypes);
        IRegistrationOptions<T> MapTypeToAbstractions(IEnumerable<Type> abstractionTypes);
        IRegistrationOptions<T> MapTypeToAllImplementedInterfaces();
        IRegistrationOptions<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression);
        IRegistrationOptions<T> UseStaticFactoryMethod(Delegate staticMethodDelegate);
        IRegistrationOptions<T> UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);
        IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptions<T> AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null);
        IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
        IRegistrationOptions<T> AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<T> ResolveInstantiationParameter<TParameter>();
        IChildRegistrationNameOptions<T> ResolveInstantiationParameter(string parameterName);
    }
}
