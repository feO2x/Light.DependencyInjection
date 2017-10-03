using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ResolveContext : IResolveContext
    {
        private ResolveDelegate _createInstance;
        private Dictionary<TypeKey, object> _perResolveInstances;
        private Registration _registration;
        private DependencyInjectionContainer _container;
        private DependencyOverrides _dependencyOverrides;

        public ResolveContext(DependencyInjectionContainer container, DependencyOverrides dependencyOverrides)
        {
            _container = container.MustNotBeNull(nameof(container));
            _dependencyOverrides = dependencyOverrides;
        }

        public object CreateInstance()
        {
            if (_createInstance == null)
                throw new InvalidOperationException("You must not call CreateInstance when IsCreatingNewInstances is set to false.");

            var instance = _createInstance(this);
            if (Registration.IsTrackingDisposables)
                Container.Scope.TryAddDisposable(instance);
            return instance;
        }

        public DependencyInjectionContainer Container => _container;

        public Registration Registration => _registration;

        public DependencyOverrides DependencyOverrides => _dependencyOverrides;

        public bool TryGetPerResolveInstance(TypeKey typeKey, out object instance)
        {
            if (_createInstance == null)
                throw new InvalidOperationException("You must not call TryGetPerResolveInstance when Lifetime.IsCreatingNewInstances is set to false.");

            if (_perResolveInstances != null)
                return _perResolveInstances.TryGetValue(typeKey, out instance);

            instance = null;
            return false;
        }

        public bool GetOrCreatePerResolveInstance(TypeKey typeKey, out object instance)
        {
            if (_createInstance == null)
                throw new InvalidOperationException("You must not call TryGetPerResolveInstance when Lifetime.IsCreatingNewInstances is set to false.");

            if (_perResolveInstances == null)
            {
                _perResolveInstances = new Dictionary<TypeKey, object>();
                goto CreateAndAddInstance;
            }

            if (_perResolveInstances.TryGetValue(typeKey, out instance))
                return false;

            CreateAndAddInstance:
            instance = CreateInstance();
            _perResolveInstances.Add(typeKey, instance);
            return true;
        }

        public object GetOrCreatePerResolveInstance(TypeKey typeKey)
        {
            GetOrCreatePerResolveInstance(typeKey, out object instance);
            return instance;
        }

        public ResolveContext ChangeResolvedType(Registration registration, ResolveDelegate createInstance)
        {
            _registration = registration.MustNotBeNull(nameof(registration));
            _createInstance = createInstance.MustNotBeNull(nameof(createInstance));
            return this;
        }

        public ResolveContext ChangeRegistration(Registration registration)
        {
            _createInstance = null;
            _registration = registration.MustNotBeNull(nameof(registration));
            return this;
        }

        public ResolveContext ChangeInitialContext(DependencyInjectionContainer container, DependencyOverrides dependencyOverrides)
        {
            _registration = null;
            _createInstance = null;
            _perResolveInstances?.Clear();
            _container = container.MustNotBeNull(nameof(container));
            _dependencyOverrides = dependencyOverrides;
            return this;
        }
    }
}