using System;
using System.Collections.Generic;
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

        public TypeAnalyzer TypeAnalyzer
        {
            get { return _typeAnalyzer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeAnalyzer = value;
            }
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

        public DiContainer RegisterSingleton<T>(string registrationName = null)
        {
            return RegisterSingleton(typeof(T), registrationName);
        }

        public DiContainer RegisterSingleton(Type type, string registrationName = null)
        {
            type.MustNotBeNull(nameof(type));

            return Register(new SingletonRegistration(_typeAnalyzer.CreateInfoFor(type), registrationName));
        }

        public DiContainer RegisterTransient<T>(string registrationName = null)
        {
            return RegisterTransient(typeof(T), registrationName);
        }

        public DiContainer RegisterTransient(Type type, string registrationName = null)
        {
            type.MustNotBeNull(nameof(type));

            return Register(new TransientRegistration(_typeAnalyzer.CreateInfoFor(type), registrationName));
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _registrations.Add(new TypeKey(registration.TargetType, registration.RegistrationName), registration);
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

            registration = _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(type));
            _registrations.Add(typeKey, registration);

            return registration.GetInstance(this);
        }
    }
}