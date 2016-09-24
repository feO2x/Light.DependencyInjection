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
        TConcreteOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo);
        TConcreteOptions UseStaticFactoryMethod(Delegate staticMethodDelegate);
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null);
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);
    }

    public interface IRegistrationOptionsForType : IBaseRegistrationOptionsForType<IRegistrationOptionsForType> { }

    public interface IRegistrationOptionsForType<T> : IBaseRegistrationOptionsForType<IRegistrationOptionsForType<T>>
    {
        IRegistrationOptionsForType<T> UseConstructorWithParameter<TParameter>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3, T4>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        IRegistrationOptionsForType<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        IRegistrationOptionsForType<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression);
        IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<IRegistrationOptionsForType<T>> ResolveInstantiationParameter<TParameter>();
    }
}