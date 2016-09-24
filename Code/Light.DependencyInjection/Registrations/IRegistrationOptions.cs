using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IBaseRegistrationOptionsForExternalInstances<out TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForExternalInstances<TConcreteOptions>
    {
        TConcreteOptions UseRegistrationName(string registrationName);
        TConcreteOptions DisableIDisposableTrackingForThisType();
        TConcreteOptions MapToAbstractions(params Type[] abstractionTypes);
        TConcreteOptions MapToAbstractions(IEnumerable<Type> abstractionTypes);
        TConcreteOptions MapToAllImplementedInterfaces();
    }

    public interface IRegistrationOptionsForExternalInstances : IBaseRegistrationOptionsForExternalInstances<IRegistrationOptionsForExternalInstances> { }

    public interface IBaseRegistrationOptionsForTypes<out TConcreteOptions> : IBaseRegistrationOptionsForExternalInstances<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForTypes<TConcreteOptions>
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

    public interface IRegistrationOptionsForTypes : IBaseRegistrationOptionsForTypes<IRegistrationOptionsForTypes> { }

    public interface IRegistrationOptionsForTypes<T> : IBaseRegistrationOptionsForTypes<IRegistrationOptionsForTypes<T>>
    {
        IRegistrationOptionsForTypes<T> UseConstructorWithParameter<TParameter>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3, T4>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3, T4, T5>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>();
        IRegistrationOptionsForTypes<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>();
        IRegistrationOptionsForTypes<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression);
        IRegistrationOptionsForTypes<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForTypes<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<IRegistrationOptionsForTypes<T>> ResolveInstantiationParameter<TParameter>();
    }
}