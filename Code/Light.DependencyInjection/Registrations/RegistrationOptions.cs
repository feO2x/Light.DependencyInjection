using System;
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
    public sealed class RegistrationOptions : IRegistrationOptions
    {
        private readonly HashSet<Type> _abstractionTypes = new HashSet<Type>();
        private readonly IConstructorSelector _constructorSelector;
        private readonly IReadOnlyList<Type> _ignoredAbstractionTypes;
        private readonly Type _targetType;
        private readonly TypeInfo _targetTypeInfo;
        private Func<object[], object> _standardizedInstantiationFunction;
        private MethodBase _creationMethodInfo;
        private string _registrationName;

        public RegistrationOptions(Type targetType, IConstructorSelector constructorSelector, IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            targetType.MustNotBeNull(nameof(targetType));
            constructorSelector.MustNotBeNull(nameof(constructorSelector));
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));

            _targetType = targetType;
            _targetTypeInfo = targetType.GetTypeInfo();
            _constructorSelector = constructorSelector;
            _ignoredAbstractionTypes = ignoredAbstractionTypes;
        }

        public string RegistrationName => _registrationName;
        public IEnumerable<Type> AbstractionTypes => _abstractionTypes;

        public IRegistrationOptions WithRegistrationName(string registrationName)
        {
            registrationName.MustNotBeNullOrEmpty(nameof(registrationName));

            _registrationName = registrationName;
            return this;
        }

        public IRegistrationOptions UseConstructor(ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            AssignConstructor(constructorInfo);
            return this;
        }

        public IRegistrationOptions UseDefaultConstructor()
        {
            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindDefaultConstructor();
            EnsureTargetConstructorIsNotNull(targetConstructor, null);
            AssignConstructor(targetConstructor);
            return this;
        }

        public IRegistrationOptions UseConstructorWithParameters(params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(parameterTypes);
            EnsureTargetConstructorIsNotNull(targetConstructor, parameterTypes);

            AssignConstructor(targetConstructor);
            return this;
        }

        public IRegistrationOptions UseConstructorWithParameter<TArgument>()
        {
            return UseConstructorWithParameters(typeof(TArgument));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return UseConstructorWithParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        public IRegistrationOptions MapTypeToAbstractions(params Type[] abstractionTypes)
        {
            return MapTypeToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        public IRegistrationOptions MapTypeToAbstractions(IEnumerable<Type> abstractionTypes)
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

        public IRegistrationOptions MapTypeToAllImplementedInterfaces()
        {
            return MapTypeToAbstractions(_targetTypeInfo.ImplementedInterfaces);
        }

        public IRegistrationOptions UseStaticFactoryMethod(Expression<Func<object>> callStaticMethodExpression)
        {
            var methodInfo = callStaticMethodExpression.ExtractStaticFactoryMethod(_targetType);
            AssignStaticCreationMethod(methodInfo);
            return this;
        }

        public IRegistrationOptions UseStaticFactoryMethod(Delegate staticMethodDelegate)
        {
            staticMethodDelegate.MustNotBeNull();

            var methodInfo = staticMethodDelegate.GetMethodInfo();
            CheckStaticCreationMethodFromDelegate(methodInfo, _targetType);
            AssignStaticCreationMethod(methodInfo);
            return this;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckStaticCreationMethodFromDelegate(MethodInfo methodInfo, Type targetType)
        {
            if (methodInfo.IsPublicStaticCreationMethodForType(targetType))
                return;

            throw new TypeRegistrationException($"The specified delegate does not describe a public, static method that returns an instance of type {targetType}.", targetType);
        }

        public IRegistrationOptions UseStaticFactoryMethod(MethodInfo staticFactoryMethodInfo)
        {
            staticFactoryMethodInfo.MustNotBeNull(nameof(staticFactoryMethodInfo));
            CheckStaticCreationMethodInfo(staticFactoryMethodInfo, _targetType);

            AssignStaticCreationMethod(staticFactoryMethodInfo);
            return this;
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

        public TypeInstantiationInfo CreateTypeInstantiationInfo()
        {
            AssignCreationMethodIfNeccessary();

            return TypeInstantiationInfo.FromTypeInstantiatedByDiContainer(_targetType, _creationMethodInfo, _standardizedInstantiationFunction);
        }

        private void AssignCreationMethodIfNeccessary()
        {
            if (_creationMethodInfo != null)
                return;

            var targetConstructor = _constructorSelector.SelectTargetConstructor(_targetTypeInfo);
            AssignConstructor(targetConstructor);
        }

        private void AssignConstructor(ConstructorInfo targetConstructor)
        {
            _standardizedInstantiationFunction = targetConstructor.CompileStandardizedInstantiationFunction();
            _creationMethodInfo = targetConstructor;
        }

        private void AssignStaticCreationMethod(MethodInfo staticCreationMethod)
        {
            _standardizedInstantiationFunction = staticCreationMethod.CompileStandardizedInstantiationFunction();
            _creationMethodInfo = staticCreationMethod;
        }
    }
}