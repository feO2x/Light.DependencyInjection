using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct CreationContext
    {
        public readonly DiContainer Container;
        public readonly ParameterOverrides? ParameterOverrides;
        public readonly Registration Registration;
        public readonly Dictionary<TypeKey, object> ResolveScope;

        private CreationContext(DiContainer container,
                                ParameterOverrides? parameterOverrides,
                                Registration registration,
                                Dictionary<TypeKey, object> resolveScope)
        {
            Container = container;
            ParameterOverrides = parameterOverrides;
            Registration = registration;
            ResolveScope = resolveScope;
        }

        private CreationContext(DiContainer container, ParameterOverrides? parameterOverrides)
        {
            Container = container;
            ParameterOverrides = parameterOverrides;
            Registration = null;
            ResolveScope = null;
        }

        public object ResolveChildValue(TypeKey requestedTypeKey)
        {
            object perResolveInstance;
            if (ResolveScope != null && ResolveScope.TryGetValue(requestedTypeKey, out perResolveInstance))
                return perResolveInstance;

            return Container.ResolveRecursively(requestedTypeKey, this);
        }

        public static CreationContext FromResolveContext(ResolveContext resolveContext, Dictionary<TypeKey, object> resolveScope)
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