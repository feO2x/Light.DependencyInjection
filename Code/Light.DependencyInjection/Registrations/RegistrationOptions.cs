using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptions : BaseRegistrationOptions<IRegistrationOptions>, IRegistrationOptions
    {
        public RegistrationOptions(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes) :
            base(targetType, constructorSelector, ignoredAbstractionTypes) { }

        public TypeCreationInfo BuildTypeCreationInfo()
        {
            AssignInstantiationMethodIfNeccessary();

            return new TypeCreationInfo(new TypeKey(TargetType, RegistrationName), InstantiationInfo, InstanceInjections);
        }
    }

    public sealed class RegistrationOptions<T> : BaseRegistrationOptions<IRegistrationOptions<T>>, IRegistrationOptions<T>
    {
        public RegistrationOptions(IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes) : base(typeof(T), constructorSelector, ignoredAbstractionTypes)
        {
            constructorSelector.MustNotBeNull(nameof(constructorSelector));
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));
        }

        public IRegistrationOptions<T> UseConstructorWithParameter<TArgument>()
        {
            return UseConstructorWithParameters(typeof(TArgument));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public IRegistrationOptions<T> UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }


        public IRegistrationOptions<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression)
        {
            var methodInfo = callStaticMethodExpression.ExtractStaticFactoryMethod(TargetType);
            AssignStaticCreationMethod(methodInfo);
            return this;
        }

        public IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null)
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            AddInstanceInjection(new PropertyInjection(selectPropertyExpression.ExtractSettableInstancePropertyInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null)
        {
            selectFieldExpression.MustNotBeNull();

            AddInstanceInjection(new FieldInjection(selectFieldExpression.ExtractSettableInstanceFieldInfo(TargetType), resolvedRegistrationName));
            return this;
        }

        public IChildRegistrationNameOptions<IRegistrationOptions<T>> ResolveInstantiationParameter<TParameter>()
        {
            AssignInstantiationMethodIfNeccessary();

            var parameterType = typeof(TParameter);
            var targetParameters = InstantiationInfo?.InstantiationDependencies?.Where(p => p.ParameterType == parameterType)
                                                    .ToList();
            CheckTargetParametersWithoutName(targetParameters, parameterType);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<IRegistrationOptions<T>>(this, targetParameters[0]);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckTargetParametersWithoutName(List<ParameterDependency> targetParameters, Type targetParameterType)
        {
            if (targetParameters == null || targetParameters.Count == 0)
                throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" does not have a parameter of type \"{targetParameterType}\".", TargetType);

            if (targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method for type \"{TargetType}\" has several parameters with type \"{targetParameterType}\". Please use the overload of \"{nameof(ResolveInstantiationParameter)}\" where an additional parameter name can be specified.", TargetType);
        }

        public TypeCreationInfo BuildTypeCreationInfo()
        {
            AssignInstantiationMethodIfNeccessary();

            return new TypeCreationInfo(new TypeKey(TargetType, RegistrationName), InstantiationInfo, InstanceInjections);
        }
    }
}