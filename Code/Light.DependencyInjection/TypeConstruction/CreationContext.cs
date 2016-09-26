using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct CreationContext
    {
        public readonly DiContainer Container;
        public readonly ParameterOverrides? ParameterOverrides;
        public readonly Registration Registration;
        public readonly Lazy<Dictionary<TypeKey, object>> LazyResolveScope;

        private CreationContext(DiContainer container, ParameterOverrides? parameterOverrides)
        {
            Container = container;
            ParameterOverrides = parameterOverrides;
            Registration = null;
            LazyResolveScope = new Lazy<Dictionary<TypeKey, object>>(CreateDictionary, LazyThreadSafetyMode.None);
        }

        private static Dictionary<TypeKey, object> CreateDictionary()
        {
            return new Dictionary<TypeKey, object>();
        }

        private CreationContext(DiContainer container,
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

        public static CreationContext CreateInitial(DiContainer container, ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));

            return new CreationContext(container, parameterOverrides);
        }
    }
}