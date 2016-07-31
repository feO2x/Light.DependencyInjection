using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer
    {
        private readonly IDictionary<TypeKey, Registration> _registrations;
        private TypeAnalyzer _typeAnalyzer = new TypeAnalyzer();
        private IDefaultRegistrationFactory _defaultRegistrationFactory = new TransientRegistrationFactory();

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

        public DiContainer RegisterSingleton<T>(string registrationName = null)
        {
            return RegisterSingleton(typeof(T), registrationName);
        }

        public DiContainer RegisterSingleton(Type type, string registrationName = null)
        {
            type.MustNotBeNull(nameof(type));

            return Register(new SingletonRegistration(_typeAnalyzer.CreateInfoFor(type), registrationName));
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _registrations.Add(new TypeKey(registration.TargetType, registration.RegistrationName), registration);
            return this;
        }

        public T Resolve<T>()
        {
            return (T) Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var typeKey = new TypeKey(type);
            Registration registration;
            if (_registrations.TryGetValue(typeKey, out registration))
                return registration.GetInstance(this);

            registration = _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(type));
            _registrations.Add(typeKey, registration);

            return registration.GetInstance(this);
        }
    }
}