using System;
using System.Collections.Generic;
using System.Linq;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Threading
{
    public sealed class ThreadSafeContainerScope : ContainerScope
    {
        private readonly object _lock = new object();

        public ThreadSafeContainerScope(ContainerScope parentScope = null)
            : base(parentScope) { }

        /// <inheritdoc />
        public override IReadOnlyList<IDisposable> DisposableObjects
        {
            get
            {
                lock (_lock)
                {
                    return base.DisposableObjects.ToList();
                }
            }
        }

        public override bool TryGetScopedInstance(TypeKey typeKey, out object instance, bool searchInParentScope = true)
        {
            lock (_lock)
            {
                return base.TryGetScopedInstance(typeKey, out instance, searchInParentScope);
            }
        }

        public override bool GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, out object instance, bool searchInParentScope = true)
        {
            lock (_lock)
            {
                return base.GetOrAddScopedInstance(typeKey, createInstance, out instance, searchInParentScope);
            }
        }

        public override bool TryAddDisposable(object instance)
        {
            lock (_lock)
            {
                return base.TryAddDisposable(instance);
            }
        }

        public override void Dispose()
        {
            lock (_lock)
            {
                base.Dispose();
            }
        }
    }
}