﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptions<T> : IRegistrationOptions<T>
    {
        private readonly HashSet<Type> _abstractionTypes = new HashSet<Type>();
        private readonly IConstructorSelector _constructorSelector;
        private readonly IReadOnlyList<Type> _ignoredAbstractionTypes;
        private readonly Type _targetType;
        private readonly TypeInfo _targetTypeInfo;
        private List<InstanceInjection> _instanceInjections;
        private MethodBase _instantiationMethodInfo;
        private List<ParameterDependency> _instantiationParameters;
        private string _registrationName;
        private Func<object[], object> _standardizedInstantiationFunction;

        public RegistrationOptions(IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            constructorSelector.MustNotBeNull(nameof(constructorSelector));
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));

            _targetType = typeof(T);
            _targetTypeInfo = _targetType.GetTypeInfo();
            _constructorSelector = constructorSelector;
            _ignoredAbstractionTypes = ignoredAbstractionTypes;
        }

        public string RegistrationName => _registrationName;
        public IEnumerable<Type> AbstractionTypes => _abstractionTypes;

        public IRegistrationOptions<T> WithRegistrationName(string registrationName)
        {
            registrationName.MustNotBeNullOrEmpty(nameof(registrationName));

            _registrationName = registrationName;
            return this;
        }

        public IRegistrationOptions<T> UseConstructor(ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            AssignConstructor(constructorInfo);
            return this;
        }

        public IRegistrationOptions<T> UseDefaultConstructor()
        {
            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindDefaultConstructor();
            EnsureTargetConstructorIsNotNull(targetConstructor, null);
            AssignConstructor(targetConstructor);
            return this;
        }

        public IRegistrationOptions<T> UseConstructorWithParameters(params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes);
            EnsureTargetConstructorIsNotNull(targetConstructor, parameterTypes);

            AssignConstructor(targetConstructor);
            return this;
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

        public IRegistrationOptions<T> MapTypeToAbstractions(params Type[] abstractionTypes)
        {
            return MapTypeToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        public IRegistrationOptions<T> MapTypeToAbstractions(IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            abstractionTypes.MustNotBeNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                if (_ignoredAbstractionTypes.Contains(abstractionType))
                    continue;

                _abstractionTypes.Add(abstractionType);
            }
            return this;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public IRegistrationOptions<T> MapTypeToAllImplementedInterfaces()
        {
            return MapTypeToAbstractions(_targetTypeInfo.ImplementedInterfaces);
        }

        public IRegistrationOptions<T> UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression)
        {
            var methodInfo = callStaticMethodExpression.ExtractStaticFactoryMethod(_targetType);
            AssignStaticCreationMethod(methodInfo);
            return this;
        }

        public IRegistrationOptions<T> UseStaticFactoryMethod(Delegate staticMethodDelegate)
        {
            staticMethodDelegate.MustNotBeNull();

            var methodInfo = staticMethodDelegate.GetMethodInfo();
            CheckStaticCreationMethodFromDelegate(methodInfo, _targetType);
            AssignStaticCreationMethod(methodInfo);
            return this;
        }

        public IRegistrationOptions<T> UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo)
        {
            staticFactoryMethodInfo.MustNotBeNull(nameof(staticFactoryMethodInfo));
            CheckStaticCreationMethodInfo(staticFactoryMethodInfo, _targetType);

            AssignStaticCreationMethod(staticFactoryMethodInfo);
            return this;
        }

        public IRegistrationOptions<T> AddPropertyInjection<TProperty>(Expression<Func<T, TProperty>> selectPropertyExpression, string resolvedRegistrationName = null)
        {
            selectPropertyExpression.MustNotBeNull(nameof(selectPropertyExpression));

            AddInstanceInjection(new PropertyInjection(selectPropertyExpression.ExtractSettableInstancePropertyInfo(_targetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptions<T> AddPropertyInjection(PropertyInfo propertyInfo, string resolvedRegistrationName = null)
        {
            propertyInfo.MustNotBeNull(nameof(propertyInfo));
            CheckPropertyInfo(propertyInfo, _targetType);

            AddInstanceInjection(new PropertyInjection(propertyInfo, resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptions<T> AddFieldInjection<TField>(Expression<Func<T, TField>> selectFieldExpression, string resolvedRegistrationName = null)
        {
            selectFieldExpression.MustNotBeNull();

            AddInstanceInjection(new FieldInjection(selectFieldExpression.ExtractSettableInstanceFieldInfo(_targetType), resolvedRegistrationName));
            return this;
        }

        public IRegistrationOptions<T> AddFieldInjection(FieldInfo fieldInfo, string resolvedRegistrationName = null)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            CheckFieldInfo(fieldInfo, _targetType);

            AddInstanceInjection(new FieldInjection(fieldInfo, resolvedRegistrationName));
            return this;
        }

        public IChildRegistrationNameOptions<T> ResolveInstantiationParameter<TParameter>()
        {
            AssignInstantiationMethodIfNeccessary();

            var parameterType = typeof(TParameter);
            var targetParameters = _instantiationParameters?.FindAll(p => p.ParameterType == parameterType);
            CheckTargetParametersWithoutName(targetParameters, parameterType);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<T>(this, targetParameters[0]);
        }

        public IChildRegistrationNameOptions<T> ResolveInstantiationParameter(string parameterName)
        {
            AssignInstantiationMethodIfNeccessary();

            var targetParameters = _instantiationParameters?.FindAll(p => p.TargetParameter.Name == parameterName);
            CheckTargetParametersWithName(targetParameters, parameterName);

            // ReSharper disable once PossibleNullReferenceException
            return new ChildRegistrationNameOptions<T>(this, targetParameters[0]);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckTargetParametersWithoutName(List<ParameterDependency> targetParameters, Type targetParameterType)
        {
            if (targetParameters == null || targetParameters.Count == 0)
                throw new TypeRegistrationException($"The specified instantiation method \"{_instantiationMethodInfo}\" for type \"{_targetType}\" does not have a parameter of type \"{targetParameterType}\".", _targetType);

            if (targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method \"{_instantiationMethodInfo}\" for type \"{_targetType}\" has several parameters with type \"{targetParameterType}\". Please use the overload of \"{nameof(ResolveInstantiationParameter)}\" where an additional parameter name can be specified.", _targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckTargetParametersWithName(List<ParameterDependency> targetParameters, string parameterName)
        {
            if (targetParameters == null || targetParameters.Count == 1)
                return;

            throw new TypeRegistrationException($"The specified instantiation method \"{_instantiationMethodInfo}\" for type \"{_targetType}\" does not have a parameter with name \"{parameterName}\".", _targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckStaticCreationMethodFromDelegate(MethodInfo methodInfo, Type targetType)
        {
            if (methodInfo.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"The specified delegate does not describe a public, static method that returns an instance of type {targetType}.", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyInfo(PropertyInfo propertyInfo, Type targetType)
        {
            if (propertyInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The property info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldInfo(FieldInfo fieldInfo, Type targetType)
        {
            if (fieldInfo.DeclaringType == targetType)
                return;

            throw new TypeRegistrationException($"The field info you provided does not belong to the target type \"{targetType}\".", targetType);
        }

        private void AddInstanceInjection(InstanceInjection instanceInjection)
        {
            if (_instanceInjections == null)
                _instanceInjections = new List<InstanceInjection>();

            _instanceInjections.Add(instanceInjection);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckStaticCreationMethodInfo(MethodInfo methodInfo, Type targetType)
        {
            if (methodInfo.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"The specified method info does not describe a public, static method that returns an instance of type {targetType}", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureTargetConstructorIsNotNull(ConstructorInfo targetConstructor, Type[] parameterTypes)
        {
            if (targetConstructor != null)
                return;

            if (parameterTypes == null || parameterTypes.Length == 0)
                throw new TypeRegistrationException($"You specified that the DI container should use the default constructor of type \"{_targetType}\", but this type contains no default constructor.", _targetType);

            if (parameterTypes.Length == 1)
                throw new TypeRegistrationException($"You specified that the DI container should use the constructor with a single parameter of type \"{parameterTypes[0]}\", but type \"{_targetType}\" does not contain such a constructor.", _targetType);

            var message = new StringBuilder().Append("You specified that the DI container should use the constructor with the type parameters ")
                                             .AppendWordEnumeration(parameterTypes)
                                             .Append($", but type \"{_targetType}\" does not contain such a constructor.")
                                             .ToString();

            throw new TypeRegistrationException(message, _targetType);
        }

        public TypeCreationInfo BuildTypeCreationInfo()
        {
            AssignInstantiationMethodIfNeccessary();

            return TypeCreationInfo.FromTypeInstantiatedByDiContainer(_targetType,
                                                                      new TypeInstantiationInfo(_targetType, _instantiationMethodInfo, _standardizedInstantiationFunction, _instantiationMethodInfo.CreateDefaultParameterDependencies()),
                                                                      _instanceInjections == null ? null : new InstanceInjectionInfo(_instanceInjections));
        }

        private void AssignInstantiationMethodIfNeccessary()
        {
            if (_instantiationMethodInfo != null)
                return;

            var targetConstructor = _constructorSelector.SelectTargetConstructor(_targetTypeInfo);
            AssignConstructor(targetConstructor);
        }

        private void AssignConstructor(ConstructorInfo targetConstructor)
        {
            _instantiationMethodInfo = targetConstructor;
            _standardizedInstantiationFunction = targetConstructor.CompileStandardizedInstantiationFunction();
            _instantiationParameters = targetConstructor.CreateDefaultParameterDependencies();
        }

        private void AssignStaticCreationMethod(MethodInfo staticCreationMethod)
        {
            _instantiationMethodInfo = staticCreationMethod;
            _standardizedInstantiationFunction = staticCreationMethod.CompileStandardizedInstantiationFunction();
            _instantiationParameters = staticCreationMethod.CreateDefaultParameterDependencies();
        }
    }
}