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
            var type = typeof(T);
            var typeKey = new TypeKey(type);
            IRegistration registration;
            if (_registrations.TryGetValue(typeKey, out registration) == false)
            {
                registration = new Transient(typeof(T));
                _registrations.Add(typeKey, registration);
            }

            return (T) registration.Create(this);
        }
    }
}