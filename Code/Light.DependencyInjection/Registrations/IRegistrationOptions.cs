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
        TConcreteOptions UseStaticFactoryMethod(Delegate staticMethodDelegate);
        TConcreteOptions UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression);
        TConcreteOptions AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null);
        TConcreteOptions AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter(string parameterName);
        IChildRegistrationNameOptions<TConcreteOptions> ResolveInstantiationParameter<TParameter>();
    }

    public interface IRegistrationOptionsForType : IBaseRegistrationOptionsForType<IRegistrationOptionsForType> { }

    public interface IRegistrationOptionsForType<T> : IBaseRegistrationOptionsForType<IRegistrationOptionsForType<T>>
    {
        IRegistrationOptionsForType<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null);
        IRegistrationOptionsForType<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null);
    }
}