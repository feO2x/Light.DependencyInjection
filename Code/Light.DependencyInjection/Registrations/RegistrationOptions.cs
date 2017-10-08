using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public class RegistrationOptions : BaseCreateInstanceOptions<IRegistrationOptions>, IRegistrationOptions
    {
        public RegistrationOptions(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes, IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector) : base(targetType, ignoredAbstractionTypes, defaultInstantiationInfoSelector) { }

        public IRegistrationOptions InstantiateVia(Func<object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T>(Func<T, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2>(Func<T1, T2, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }
    }

    public class RegistrationOptions<T> : BaseCreateInstanceOptions<IRegistrationOptions<T>>, IRegistrationOptions<T>
    {
        public RegistrationOptions(IReadOnlyList<Type> ignoredAbstractionTypes, IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector) : base(typeof(T), ignoredAbstractionTypes, defaultInstantiationInfoSelector) { }

        public IRegistrationOptions<T> InstantiateVia(Func<T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1>(Func<T1, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2>(Func<T1, T2, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3>(Func<T1, T2, T3, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> InstantiateVia<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> createInstance)
        {
            return SetDelegateInstantiationInfoFactory(createInstance);
        }

        public IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string targetRegistrationName = "")
        {
            var property = selectPropertyExpression.ExtractProperty();
            return AddPropertyInjection(property, targetRegistrationName);
        }

        public IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, Action<IDependencyOptions> configureDependency)
        {
            var property = selectPropertyExpression.ExtractProperty();
            return AddPropertyInjection(property, configureDependency);
        }

        public IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string targetRegistrationName = "")
        {
            var field = selectFieldExpression.ExtractField();
            return AddFieldInjection(field, targetRegistrationName);
        }

        public IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, Action<IDependencyOptions> configureDependency)
        {
            var field = selectFieldExpression.ExtractField();
            return AddFieldInjection(field, configureDependency);
        }
    }
}