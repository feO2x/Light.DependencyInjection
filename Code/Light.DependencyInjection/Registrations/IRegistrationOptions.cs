using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IBaseRegistrationOptions<out TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptions<TConcreteOptions>
    {
        TConcreteOptions UseRegistrationName(string registrationName);
        TConcreteOptions UseConstructor(ConstructorInfo constructorInfo);
        TConcreteOptions UseDefaultConstructor();
        TConcreteOptions UseConstructorWithParameters(params Type[] parameterTypes);
        TConcreteOptions MapTypeToAbstractions(params Type[] abstractionTypes);
        TConcreteOptions MapTypeToAbstractions(IEnumerable<Type> abstractionTypes);
        TConcreteOptions MapTypeToAllImplementedInterfaces();
        TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);
        TConcreteOptions UseStaticFactoryMethod(Delegate staticMethodDelegate);
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null);
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);
    }

    public interface IRegistrationOptions : IBaseRegistrationOptions<IRegistrationOptions> { }

    public interface IRegistrationOptions<T> : IBaseRegistrationOptions<IRegistrationOptions<T>>
    {
        IRegistrationOptions<T> UseConstructorWithParameter<TParameter>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        IRegistrationOptions<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression);
        IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<IRegistrationOptions<T>> ResolveInstantiationParameter<TParameter>();
    }
}