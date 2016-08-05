using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class RegistrationOptions<T> : IRegistrationOptions
    {
        private readonly IConstructorSelector _constructorSelector;
        private readonly Type _targetType = typeof(T);
        private readonly TypeInfo _targetTypeInfo = typeof(T).GetTypeInfo();
        private Func<object[], object> _compiledCreationMethod;
        private MethodBase _creationMethodInfo;
        private string _registrationName;

        public RegistrationOptions(IConstructorSelector constructorSelector)
        {
            constructorSelector.MustNotBeNull(nameof(constructorSelector));

            _constructorSelector = constructorSelector;
        }

        public string RegistrationName => _registrationName;

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

            return TypeInstantiationInfo.FromTypeInstantiatedByDiContainer(typeof(T), _creationMethodInfo, _compiledCreationMethod);
        }

        private void AssignCreationMethodIfNeccessary()
        {
            if (_creationMethodInfo != null)
                return;

            var targetConstructor = _constructorSelector.SelectTargetConstructor(typeof(T).GetTypeInfo());
            AssignConstructor(targetConstructor);
        }

        private void AssignConstructor(ConstructorInfo targetConstructor)
        {
            _compiledCreationMethod = targetConstructor.CompileObjectCreationFunction();
            _creationMethodInfo = targetConstructor;
        }
    }
}