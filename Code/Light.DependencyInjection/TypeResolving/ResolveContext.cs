using System;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public struct ResolveContext
    {
        public readonly Registration Registration;
        public readonly ContainerScope Scope;
        private readonly Func<object> _createInstance;

        public ResolveContext(Func<object> createInstance, ContainerScope scope, Registration registration)
        {
            _createInstance = createInstance.MustNotBeNull(nameof(createInstance));
            Scope = scope.MustNotBeNull(nameof(scope));
            Registration = registration.MustNotBeNull(nameof(registration));
        }

        public object CreateInstance()
        {
            if (_createInstance == null)
                throw new InvalidOperationException("You must not call CreateInstance when IsCreatingNewInstances is set to true.");

            var instance = _createInstance();
            if (Registration.IsTrackingDisposables)
                Scope.TryAddDisposable(instance);
            return instance;
        }
    }
}