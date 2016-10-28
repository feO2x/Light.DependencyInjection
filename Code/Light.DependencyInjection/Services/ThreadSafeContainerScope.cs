using System;
using System.Collections.Generic;
using System.Linq;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents a thread-safe version of the <see cref="ContainerScope" />.
    /// </summary>
    public sealed class ThreadSafeContainerScope : ContainerScope
    {
        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of <see cref="ThreadSafeContainerScope" />.
        /// </summary>
        /// <param name="parentScope">The parent scope associated with the new one.</param>
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

        /// <inheritdoc />
        public override bool TryGetScopedInstance(TypeKey typeKey, out object instance)
        {
            lock (_lock)
            {
                return base.TryGetScopedInstance(typeKey, out instance);
            }
        }

        /// <inheritdoc />
        public override bool GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, out object instance)
        {
            lock (_lock)
            {
                return base.GetOrAddScopedInstance(typeKey, createInstance, out instance);
            }
        }

        /// <inheritdoc />
        public override bool TryAddDisposable(object instance)
        {
            lock (_lock)
            {
                return base.TryAddDisposable(instance);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            lock (_lock)
            {
                base.Dispose();
            }
        }
    }
}