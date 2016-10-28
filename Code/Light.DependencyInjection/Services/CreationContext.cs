using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents all information necessary to instantiate a registration.
    /// </summary>
    public struct CreationContext
    {
        /// <summary>
        ///     Gets the <see cref="DependencyInjectionContainer" /> instance.
        /// </summary>
        public readonly DependencyInjectionContainer Container;

        /// <summary>
        ///     Gets the parameter overrides for this resolve call (if present).
        /// </summary>
        public readonly ParameterOverrides? ParameterOverrides;

        /// <summary>
        ///     Gets the registration that should be instantiated.
        /// </summary>
        public readonly Registration Registration;

        /// <summary>
        ///     Gets the scope of this resolve call.
        /// </summary>
        public readonly Lazy<Dictionary<TypeKey, object>> LazyResolveScope;

        private CreationContext(DependencyInjectionContainer container, ParameterOverrides? parameterOverrides)
        {
            Container = container;
            ParameterOverrides = parameterOverrides;
            Registration = null;
            LazyResolveScope = container.Services.ResolveScopeFactory.CreateLazyScope();
        }

        private CreationContext(DependencyInjectionContainer container,
                                ParameterOverrides? parameterOverrides,
                                Registration registration,
                                Lazy<Dictionary<TypeKey, object>> lazyResolveScope)
        {
            Container = container;
            ParameterOverrides = parameterOverrides;
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
        ///     Creates a <see cref="CreationContext" /> instance from the specified <see cref="ResolveContext" />.
        /// </summary>
        public static CreationContext FromResolveContext(ResolveContext resolveContext, Lazy<Dictionary<TypeKey, object>> resolveScope)
        {
            return new CreationContext(resolveContext.Container,
                                       resolveContext.ParameterOverrides,
                                       resolveContext.Registration,
                                       resolveScope);
        }

        /// <summary>
        ///     Creates the <see cref="CreationContext" /> instance for the initial call to <see cref="DependencyInjectionContainer.ResolveRecursively" />.
        /// </summary>
        public static CreationContext CreateInitial(DependencyInjectionContainer container, ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));

            return new CreationContext(container, parameterOverrides);
        }
    }
}