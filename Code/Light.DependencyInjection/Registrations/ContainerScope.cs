using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Light.DependencyInjection.Multithreading;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ContainerScope : IDisposable
    {
        private readonly List<IDisposable> _disposableObjects = new List<IDisposable>();
        private readonly FastReadThreadSafeDictionary<TypeKey, object> _singletons = new FastReadThreadSafeDictionary<TypeKey, object>();
        public readonly ContainerScope ParentScope;

        public ContainerScope() { }

        public ContainerScope(ContainerScope parentScope)
        {
            parentScope.MustNotBeNull(nameof(parentScope));
            parentScope.MustNotBeSameAs(this, nameof(parentScope));

            ParentScope = parentScope;
        }

        public IReadOnlyList<IDisposable> DisposableObjects => _disposableObjects.ToList();

        public void Dispose()
        {
            foreach (var disposable in _disposableObjects)
            {
                disposable.Dispose();
            }
        }

        public void TryAddDisposable(object @object)
        {
            var disposable = @object as IDisposable;
            if (disposable == null)
                return;

            Monitor.Enter(_disposableObjects);
            _disposableObjects.Add(disposable);
            Monitor.Exit(_disposableObjects);
        }

        public bool TryGetInstance(TypeKey key, out object instance)
        {
            if (_singletons.TryGetValue(key, out instance))
                return true;

            return ParentScope != null && ParentScope.TryGetInstance(key, out instance);
        }

        public bool GetOrAddInstance(TypeKey typeKey, Func<object> createInstance, out object instance)
        {
            if (ParentScope != null && ParentScope.TryGetInstance(typeKey, out instance))
                return false;

            return _singletons.GetOrAdd(typeKey, createInstance, out instance);
        }

        public object GetOrAddInstance(TypeKey typeKey, Func<object> createInstance)
        {
            object instance;
            GetOrAddInstance(typeKey, createInstance, out instance);
            return instance;
        }
    }
}