﻿using System;
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
        private readonly SynchronizedDictionary<TypeKey, object> _singletons = new SynchronizedDictionary<TypeKey, object>();
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

        public bool TryGetSingleton(TypeKey key, out object singleton)
        {
            if (_singletons.TryGetValue(key, out singleton))
                return true;

            return ParentScope != null && ParentScope.TryGetSingleton(key, out singleton);
        }

        public bool GetOrAddSingleton(TypeKey typeKey, Func<object> createInstance, out object singleton)
        {
            if (ParentScope != null && ParentScope.TryGetSingleton(typeKey, out singleton))
                return false;

            return _singletons.GetOrAdd(typeKey, createInstance, out singleton);
        }

        public object GetOrAddSingleton(TypeKey typeKey, Func<object> createInstance)
        {
            object singleton;
            GetOrAddSingleton(typeKey, createInstance, out singleton);
            return singleton;
        }
    }
}