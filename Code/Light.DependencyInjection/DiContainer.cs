using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer
    {
        private readonly IDictionary<TypeKey, Registration> _registrations;
        private IDefaultRegistrationFactory _defaultRegistrationFactory = new TransientRegistrationFactory();
        private TypeAnalyzer _typeAnalyzer = new TypeAnalyzer();

        public DiContainer() : this(new Dictionary<TypeKey, Registration>()) { }

        public DiContainer(IDictionary<TypeKey, Registration> registrations)
        {
            registrations.MustNotBeNull(nameof(registrations));

            _registrations = registrations;
        }

        public IDefaultRegistrationFactory DefaultRegistrationFactory
        {
            get { return _defaultRegistrationFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _defaultRegistrationFactory = value;
            }
        }

        public ICollection<Registration> Registrations => _registrations.Values;

        public TypeAnalyzer TypeAnalyzer
        {
            get { return _typeAnalyzer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeAnalyzer = value;
            }
        }

        public DiContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            Register(registration);

            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _registrations.Add(new TypeKey(abstractionType, registration.Name), registration);
            }

            return this;
        }

        public DiContainer Register(Registration registration, params Type[] abstractionTypes)
        {
            return Register(registration, (IEnumerable<Type>) abstractionTypes);
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _registrations.Add(new TypeKey(registration.TargetType, registration.Name), registration);

            return this;
        }

        public T Resolve<T>(string registrationName = null)
        {
            return (T) Resolve(typeof(T), registrationName);
        }

        public object Resolve(Type type, string registrationName = null)
        {
            var registration = GetRegistration(type, registrationName);
            if (registration != null)
                return registration.GetInstance(this);

            registration = CreateDefaultRegistration(type, registrationName);
            return registration.GetInstance(this);
        }

        public T Resolve<T>(ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return (T) Resolve(typeof(T), parameterOverrides, registrationName);
        }

        public object Resolve(Type type, ParameterOverrides parameterOverrides, string registrationName = null)
        {
            var registration = GetRegistration(type, registrationName);
            if (registration != null)
                return registration.CreateInstance(this, parameterOverrides);

            registration = CreateDefaultRegistration(type, registrationName);
            return registration.CreateInstance(this, parameterOverrides);
        }

        private Registration CreateDefaultRegistration(Type type, string registrationName)
        {
            CheckIfTypeIsInstantiable(type);

            var registration = _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(type));
            _registrations.Add(new TypeKey(type, registrationName), registration);
            return registration;
        }

        public ParameterOverrides OverrideParametersFor<T>(string registrationName = null)
        {
            return OverrideParametersFor(typeof(T), registrationName);
        }

        public ParameterOverrides OverrideParametersFor(Type type, string registrationName = null)
        {
            var targetRegistration = GetRegistration(type, registrationName);
            CheckTargetRegistration(targetRegistration, new TypeKey(type, registrationName));

            return new ParameterOverrides(targetRegistration.TypeCreationInfo);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        // ReSharper disable once UnusedParameter.Local
        private static void CheckTargetRegistration(Registration targetRegistration, TypeKey typeKey)
        {
            var registrationNameText = typeKey.RegistrationName == null ? "" : $" with registration name {typeKey.RegistrationName}";
            if (targetRegistration == null)
                throw new ResolveTypeException($"The type \"{typeKey.Type}\"{registrationNameText} is not registered with the DI container.", typeKey.Type);
            if (targetRegistration.IsCreatingNewInstanceOnNextResolve == false)
                throw new ResolveTypeException($"The type \"{typeKey.Type}\"{registrationNameText} will not be instantiated on the next resolve call, but use an existing instance. Thus it's parameters cannot be overridden.", typeKey.Type);
            if (targetRegistration.TypeCreationInfo == null)
                throw new ResolveTypeException($"The type \"{typeKey.Type}\"{registrationNameText} is not instantiated by the DI container and thus cannot have it's parameters overridden.", typeKey.Type);
        }

        public Registration GetRegistration<T>(string registrationName = null)
        {
            return GetRegistration(typeof(T), registrationName);
        }

        public Registration GetRegistration(Type type, string registrationName = null)
        {
            var typeKey = new TypeKey(type, registrationName);
            Registration registration;
            if (_registrations.TryGetValue(typeKey, out registration))
                return registration;

            if (type.IsConstructedGenericType == false)
                return null;

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericTypeDefinitionKey = new TypeKey(genericTypeDefinition, registrationName);
            if (_registrations.TryGetValue(genericTypeDefinitionKey, out registration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == registration.TargetType ? type : registration.TargetType.MakeGenericType(type.GenericTypeArguments);
            registration = registration.BindGenericTypeDefinition(closedConstructedType);
            _registrations.Add(typeKey, registration);
            return registration;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeIsInstantiable(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            // TODO: create different error messages for different scenarios
            if (typeInfo.IsInterface || typeInfo.IsAbstract || typeInfo.IsEnum || typeInfo.BaseType == typeof(MulticastDelegate))
                throw new TypeRegistrationException($"The specified type \"{type}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.", type);
        }
    }
}