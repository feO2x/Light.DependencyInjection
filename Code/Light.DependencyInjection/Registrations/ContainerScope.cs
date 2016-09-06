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
        public readonly SynchronizedDictionary<TypeKey, object> Singletons = new SynchronizedDictionary<TypeKey, object>();
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
    }
}