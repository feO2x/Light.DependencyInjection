using System;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public struct ResolveContext
    {
        public readonly Registration Registration;
        public readonly DiContainer Container;
        private readonly Func<DiContainer, object> _createInstance;

        public ResolveContext(Func<DiContainer, object> createInstance, DiContainer container, Registration registration)
        {
            _createInstance = createInstance.MustNotBeNull(nameof(createInstance));
            Container = container.MustNotBeNull(nameof(container));
            Registration = registration.MustNotBeNull(nameof(registration));
        }

        public object CreateInstance()
        {
            if (_createInstance == null)
                throw new InvalidOperationException("You must not call CreateInstance when IsCreatingNewInstances is set to true.");

            var instance = _createInstance(Container);
            if (Registration.IsTrackingDisposables)
                Container.ContainerScope.TryAddDisposable(instance);
            return instance;
        }
    }
}