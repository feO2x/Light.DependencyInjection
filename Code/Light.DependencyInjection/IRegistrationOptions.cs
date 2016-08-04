using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public interface IRegistrationOptions
    {
        IRegistrationOptions WithRegistrationName(string registrationName);
    }

    public sealed class TypeInstantiationInfoBuilder<T> : IRegistrationOptions
    {
        private string _registrationName;
        private MethodBase _creationMethodInfo;
        private Func<object[], object> _compiledCreationMethod;

        private readonly IConstructorSelector _constructorSelector;

        public TypeInstantiationInfoBuilder(IConstructorSelector constructorSelector)
        {
            constructorSelector.MustNotBeNull(nameof(constructorSelector));

            _constructorSelector = constructorSelector;
        }

        public IRegistrationOptions WithRegistrationName(string registrationName)
        {
            registrationName.MustNotBeNullOrEmpty(nameof(registrationName));

            _registrationName = registrationName;
            return this;
        }

        public string RegistrationName => _registrationName;

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