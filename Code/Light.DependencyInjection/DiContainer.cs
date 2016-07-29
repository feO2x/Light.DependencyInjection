using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer
    {
        private readonly IDictionary<TypeKey, IRegistration> _registrations;

        public DiContainer() : this(new Dictionary<TypeKey, IRegistration>()) { }

        public DiContainer(IDictionary<TypeKey, IRegistration> registrations)
        {
            registrations.MustNotBeNull(nameof(registrations));

            _registrations = registrations;
        }

        public DiContainer RegisterSingleton<T>()
        {
            _registrations.Add(new TypeKey(typeof(T)), new Singleton(typeof(T)));
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
            IRegistration registration;
            if (_registrations.TryGetValue(typeKey, out registration) == false)
            {
                registration = new Transient(type);
                _registrations.Add(typeKey, registration);
            }

            return registration.Create(this);
        }
    }
}