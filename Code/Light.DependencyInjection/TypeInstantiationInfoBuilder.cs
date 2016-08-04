using System;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class TypeInstantiationInfoBuilder<T> : IRegistrationOptions
    {
        private readonly IConstructorSelector _constructorSelector;
        private Func<object[], object> _compiledCreationMethod;
        private MethodBase _creationMethodInfo;
        private string _registrationName;
        private readonly Type _targetType = typeof(T);
        private readonly TypeInfo _targetTypeInfo = typeof(T).GetTypeInfo();

        public TypeInstantiationInfoBuilder(IConstructorSelector constructorSelector)
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

        public IRegistrationOptions UseDefaultConstructor()
        {
            var targetConstructor = typeof(T).GetTypeInfo()
                                             .DeclaredConstructors
                                             .FirstOrDefault(constructor => constructor.GetParameters().Length == 0);
            targetConstructor.MustNotBeNull(exception: 
                () => new TypeRegistrationException($"You specified that the DI container should use the default constructor of type \"{_targetType}\", but this type contains no default constructor.", _targetType));

            _compiledCreationMethod = targetConstructor.CompileObjectCreationFunction();
            _creationMethodInfo = targetConstructor;

            return this;
        }

        public TypeInstantiationInfo Build()
        {
            AssignCreationMethodIfNeccessary();

            return TypeInstantiationInfo.FromTypeInstantiatedByDiContainer(typeof(T), _creationMethodInfo, _compiledCreationMethod);
        }

        private void AssignCreationMethodIfNeccessary()
        {
            if (_creationMethodInfo != null)
                return;

            var targetConstructor = _constructorSelector.SelectTargetConstructor(typeof(T).GetTypeInfo());
            _compiledCreationMethod = targetConstructor.CompileObjectCreationFunction();
            _creationMethodInfo = targetConstructor;
        }
    }
}