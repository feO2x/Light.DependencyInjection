using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public struct CreationContext
    {
        public readonly DependencyInjectionContainer Container;
        public readonly ParameterOverrides? ParameterOverrides;
        public readonly Registration Registration;
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

        public object ResolveChildValue(TypeKey requestedTypeKey)
        {
            object perResolveInstance;
            if (LazyResolveScope.IsValueCreated && LazyResolveScope.Value.TryGetValue(requestedTypeKey, out perResolveInstance))
                return perResolveInstance;

            return Container.ResolveRecursively(requestedTypeKey, this);
        }

        public static CreationContext FromResolveContext(ResolveContext resolveContext, Lazy<Dictionary<TypeKey, object>> resolveScope)
        {
            return new CreationContext(resolveContext.Container,
                                       resolveContext.ParameterOverrides,
                                       resolveContext.Registration,
                                       resolveScope);
        }

        public static CreationContext CreateInitial(DependencyInjectionContainer container, ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));

            return new CreationContext(container, parameterOverrides);
        }
    }
}