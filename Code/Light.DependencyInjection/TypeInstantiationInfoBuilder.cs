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
            var targetConstructor = _targetTypeInfo.DeclaredConstructors
                                                   .FirstOrDefault(constructor => constructor.GetParameters().Length == 0);
            targetConstructor.MustNotBeNull(exception: 
                () => new TypeRegistrationException($"You specified that the DI container should use the default constructor of type \"{_targetType}\", but this type contains no default constructor.", _targetType));

            AssignConstructor(targetConstructor);

            return this;
        }

        public IRegistrationOptions UseConstructorWithParameter<TArgument>()
        {
            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(typeof(TArgument));
            targetConstructor.MustNotBeNull(exception:
                () => new TypeRegistrationException($"You specified that the DI container should use the constructor with a single parameter of type \"{typeof(TArgument)}\", but type \"{_targetType}\" does not contain such a constructor.", _targetType));

            AssignConstructor(targetConstructor);

            return this;
        }

        public IRegistrationOptions UseConstructorWithParameters<T1, T2>()
        {
            var targetConstructor = _targetTypeInfo.DeclaredConstructors.FindConstructorWithArgumentTypes(typeof(T1), typeof(T2));
            targetConstructor.MustNotBeNull(exception:
                () => new TypeRegistrationException($"You specified that the DI container should use the constructor with the type parameters \"{typeof(T1)}\" and \"{typeof(T2)}\", but type \"{_targetType}\" does not contain such a constructor.", _targetType));

            AssignConstructor(targetConstructor);

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
            AssignConstructor(targetConstructor);
        }

        private void AssignConstructor(ConstructorInfo targetConstructor)
        {
            _compiledCreationMethod = targetConstructor.CompileObjectCreationFunction();
            _creationMethodInfo = targetConstructor;
        }
    }
}