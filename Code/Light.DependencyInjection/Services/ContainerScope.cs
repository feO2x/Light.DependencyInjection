using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents the scope of a <see cref="DependencyInjectionContainer" /> instance, tracking objects associated with a <see cref="ScopedLifetime" />
    ///     and <see cref="IDisposable" /> instances. This class is not thread-safe, thus if you want to access the same DI Container on different
    ///     threads, you should exchange the <see cref="ContainerServices.ContainerScopeFactory" /> with an instance of <see cref="ThreadSafeContainerScopeFactory" />.
    /// </summary>
    public class ContainerScope : IDisposable
    {
        private readonly List<IDisposable> _disposableObjects = new List<IDisposable>();
        private readonly Dictionary<TypeKey, object> _scopedObjects = new Dictionary<TypeKey, object>();

        /// <summary>
        ///     Gets the parent score of this one. Will return null if the corresponding DI container is no child container.
        /// </summary>
        public readonly ContainerScope ParentScope;

        /// <summary>
        ///     Initializes a new instance of <see cref="ContainerScope" />.
        /// </summary>
        /// <param name="parentScope">The parent scope associated with the new one.</param>
        public ContainerScope(ContainerScope parentScope = null)
        {
            ParentScope = parentScope;
        }

        /// <summary>
        ///     Gets the list of all <see cref="IDisposable" /> instances being tracked by this scope.
        /// </summary>
        public virtual IReadOnlyList<IDisposable> DisposableObjects => _disposableObjects;

        /// <summary>
        ///     Disposes of this scope and all tracked <see cref="DisposableObjects" />.
        /// </summary>
        public virtual void Dispose()
        {
            for (var i = 0; i < DisposableObjects.Count; i++)
            {
                DisposableObjects[i].Dispose();
            }
        }

        /// <summary>
        ///     Tries to retrieve a scoped instance with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the scoped instance.</param>
        /// <param name="instance">The retrieved instance.</param>
        /// <returns>True if the scoped instance could be found, else false.</returns>
        public virtual bool TryGetScopedInstance(TypeKey typeKey, out object instance)
        {
            if (_scopedObjects.TryGetValue(typeKey, out instance))
                return true;

            return ParentScope != null && ParentScope.TryGetScopedInstance(typeKey, out instance);
        }

        /// <summary>
        ///     Gets or adds the specified instance to the scope.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the scoped instance.</param>
        /// <param name="createInstance">The delegate that will create the instance if necessary.</param>
        /// <param name="instance">The retrieved or created instance.</param>
        /// <returns>True if the instance was created, false when the instance was already found.</returns>
        public virtual bool GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance, out object instance)
        {
            createInstance.MustNotBeNull(nameof(createInstance));

            if (TryGetScopedInstance(typeKey, out instance))
                return false;

            instance = createInstance();
            _scopedObjects.Add(typeKey, instance);
            return true;
        }

        /// <summary>
        ///     Gets or adds the specified instance to the scope.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the scoped instance.</param>
        /// <param name="createInstance">The delegate that will create the instance if necessary.</param>
        /// <returns>The retrieved or created instance.</returns>
        public object GetOrAddScopedInstance(TypeKey typeKey, Func<object> createInstance)
        {
            object instance;
            GetOrAddScopedInstance(typeKey, createInstance, out instance);
            return instance;
        }

        /// <summary>
        ///     Tries to track the specified instance as an <see cref="IDisposable" />.
        /// </summary>
        /// <returns>True if the instance is an <see cref="IDisposable" />, else false.</returns>
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