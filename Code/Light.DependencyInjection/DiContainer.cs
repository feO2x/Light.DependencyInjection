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
            type.MustNotBeNull(nameof(type));

            var typeKey = new TypeKey(type, registrationName);
            Registration registration;
            if (_registrations.TryGetValue(typeKey, out registration))
                return registration.GetInstance(this);

            if (type.IsConstructedGenericType)
            {
                var genericTypeDefintion = type.GetGenericTypeDefinition();
                var genericTypeDefinitionKey = new TypeKey(genericTypeDefintion, registrationName);
                if (_registrations.TryGetValue(genericTypeDefinitionKey, out registration))
                {
                    var closedConstructedType = genericTypeDefintion == registration.TargetType ? type : registration.TargetType.MakeGenericType(type.GenericTypeArguments);
                    registration = registration.BindGenericTypeDefinition(closedConstructedType);
                    _registrations.Add(typeKey, registration);
                    return registration.GetInstance(this);
                }
            }

            CheckIfTypeIsInstantiable(type);

            registration = _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(type));
            _registrations.Add(typeKey, registration);

            return registration.GetInstance(this);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeIsInstantiable(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface || typeInfo.IsAbstract || typeInfo.IsEnum || typeInfo.BaseType == typeof(MulticastDelegate))
                throw new TypeRegistrationException($"The specified type \"{type}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.", type);
        }
    }
}