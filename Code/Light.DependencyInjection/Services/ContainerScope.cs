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

        public virtual bool TryGetScopedInstance(TypeKey typeKey, out object instance)
        {
            if (_scopedObjects.TryGetValue(typeKey, out instance))
                return true;

            return ParentScope != null && ParentScope.TryGetScopedInstance(typeKey, out instance);
        }

        public virtual bool GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, out object instance)
        {
            createInstance.MustNotBeNull(nameof(createInstance));

            if (TryGetScopedInstance(typeKey, out instance))
                return false;

            instance = createInstance();
            _scopedObjects.Add(typeKey, instance);
            return true;
        }

        public object GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance)
        {
            object instance;
            GetOrAddScopedInstance(typeKey, createInstance, out instance);
            return instance;
        }

        public virtual bool TryAddDisposable(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable == null)
                return false;

            _disposableObjects.Add(disposable);
            return true;
        }
    }
}