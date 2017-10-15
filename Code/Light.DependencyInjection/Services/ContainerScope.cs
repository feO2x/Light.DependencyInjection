using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public class ContainerScope : IDisposable
    {
        private readonly List<IDisposable> _disposableObjects = new List<IDisposable>();
        private readonly Dictionary<TypeKey, object> _scopedObjects = new Dictionary<TypeKey, object>();
        public readonly ContainerScope ParentScope;

        public ContainerScope(ContainerScope parentScope = null)
        {
            ParentScope = parentScope;
        }

        public virtual IReadOnlyList<IDisposable> DisposableObjects => _disposableObjects;

        public virtual void Dispose()
        {
            foreach (var disposableObject in _disposableObjects)
            {
                disposableObject.Dispose();
            }
        }

        public virtual bool TryGetScopedInstance(TypeKey typeKey, out object instance, bool searchInParentScope = true)
        {
            if (_scopedObjects.TryGetValue(typeKey, out instance))
                return true;

            return searchInParentScope && ParentScope != null && ParentScope.TryGetScopedInstance(typeKey, out instance);
        }

        public virtual bool GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, out object instance, bool searchInParentScope = true)
        {
            createInstance.MustNotBeNull(nameof(createInstance));

            if (TryGetScopedInstance(typeKey, out instance, searchInParentScope))
                return false;

            instance = createInstance();
            _scopedObjects.Add(typeKey, instance);
            return true;
        }

        public object GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, bool searchInParentScope = true)
        {
            GetOrAddScopedInstance(typeKey, createInstance, out var instance, searchInParentScope);
            return instance;
        }

        public virtual bool TryAddDisposable(object instance)
        {
            if (!(instance.MustNotBeNull(nameof(instance)) is IDisposable disposable))
                return false;

            _disposableObjects.Add(disposable);
            return true;
        }

        public virtual ContainerScope AddDisposable(IDisposable disposable)
        {
            _disposableObjects.Add(disposable.MustNotBeNull(nameof(disposable)));
            return this;
        }

        public virtual bool AddOrUpdateScopedInstance(TypeKey typeKey, object value)
        {
            var returnValue = _scopedObjects.ContainsKey(typeKey);
            _scopedObjects[typeKey] = value;
            return !returnValue;
        }
    }
}