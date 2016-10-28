using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents all information necessary to resolve a child value.
    /// </summary>
    public struct ResolveContext
    {
        /// <summary>
        ///     Gets the <see cref="DependencyInjectionContainer" /> instance.
        /// </summary>
        public readonly DependencyInjectionContainer Container;

        /// <summary>
        ///     Gets the dependency overrides for this resolve call (if present).
        /// </summary>
        public readonly DependencyOverrides? DependencyOverrides;

        /// <summary>
        ///     Gets the registration of the current resolve call.
        /// </summary>
        public readonly Registration Registration;

        /// <summary>
        ///     Gets the scope of this resolve call.
        /// </summary>
        public readonly Lazy<Dictionary<TypeKey, object>> LazyResolveScope;

        private ResolveContext(DependencyInjectionContainer container, DependencyOverrides? dependencyOverrides)
        {
            Container = container;
            DependencyOverrides = dependencyOverrides;
            Registration = null;
            LazyResolveScope = container.Services.ResolveScopeFactory.CreateLazyScope();
        }

        private ResolveContext(DependencyInjectionContainer container,
                               DependencyOverrides? dependencyOverrides,
                               Registration registration,
                               Lazy<Dictionary<TypeKey, object>> lazyResolveScope)
        {
            Container = container;
            DependencyOverrides = dependencyOverrides;
            Registration = registration;
            LazyResolveScope = lazyResolveScope;
        }

        /// <summary>
        ///     Checks if the requested type key is part of the resolve scope. If not, then a recursive resolve is performed on the DI container.
        /// </summary>
        /// <param name="requestedTypeKey">The type key uniquely identifying the requested instance.</param>
        /// <returns>The retrieved or resolved instance.</returns>
        public object ResolveChildValue(TypeKey requestedTypeKey)
        {
            object perResolveInstance;
            if (LazyResolveScope.IsValueCreated && LazyResolveScope.Value.TryGetValue(requestedTypeKey, out perResolveInstance))
                return perResolveInstance;

            return Container.ResolveRecursively(requestedTypeKey, this);
        }

        /// <summary>
        ///     Creates a <see cref="ResolveContext" /> instance from the specified <see cref="CreationContext" />.
        /// </summary>
        public static ResolveContext FromCreationContext(CreationContext creationContext, Lazy<Dictionary<TypeKey, object>> resolveScope)
        {
            return new ResolveContext(creationContext.Container,
                                      creationContext.DependencyOverrides,
                                      creationContext.Registration,
                                      resolveScope);
        }

        /// <summary>
        ///     Creates the <see cref="ResolveContext" /> instance for the initial call to <see cref="DependencyInjectionContainer.ResolveRecursively" />.
        /// </summary>
        public static ResolveContext CreateInitial(DependencyInjectionContainer container, DependencyOverrides? dependencyOverrides = null)
        {
            container.MustNotBeNull(nameof(container));

            return new ResolveContext(container, dependencyOverrides);
        }
    }
}