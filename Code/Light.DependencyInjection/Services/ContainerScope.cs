using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public class ContainerScope : IDisposable
    {
        public readonly ContainerScope ParentScope;

        public ContainerScope(ContainerScope parentScope = null)
            : this(new List<IDisposable>(), new Dictionary<TypeKey, object>(), parentScope) { }

        protected ContainerScope(IList<IDisposable> disposableObjects,
                                 IDictionary<TypeKey, object> scopedObjects,
                                 ContainerScope parentScope = null)
        {
            disposableObjects.MustNotBeNull(nameof(disposableObjects));
            scopedObjects.MustNotBeNull(nameof(scopedObjects));
            DisposableObjects = disposableObjects.MustBeOfType<IReadOnlyList<IDisposable>>();
            _disposableObjects = disposableObjects;
            _scopedObjects = scopedObjects;
            ParentScope = parentScope;
        }

        public virtual IReadOnlyList<IDisposable> DisposableObjects { get; }

        public void Dispose()
        {
            for (var i = 0; i < DisposableObjects.Count; i++)
            {
                DisposableObjects[i].Dispose();
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

        // ReSharper disable InconsistentNaming
        protected readonly IList<IDisposable> _disposableObjects;
        protected readonly IDictionary<TypeKey, object> _scopedObjects;
        // ReSharper restore InconsistentNaming
    }
}