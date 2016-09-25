using System;
using System.Collections.Generic;
using System.Diagnostics;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct ResolveContext
    {
        public readonly DiContainer Container;
        public readonly Registration Registration;
        public readonly ParameterOverrides? ParameterOverrides;
        private Dictionary<TypeKey, object> _resolveScope;

        public ResolveContext(DiContainer container,
                              Registration registration,
                              Dictionary<TypeKey, object> resolveScope,
                              ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));
            registration.MustNotBeNull(nameof(registration));

            Container = container;
            Registration = registration;
            _resolveScope = resolveScope;
            ParameterOverrides = parameterOverrides;
        }

        public object CreateInstance()
        {
            EnsureTypeCreationInfoIsNotNull();

            return Registration.TypeCreationInfo.CreateInstance(CreationContext.FromResolveContext(this, _resolveScope));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureTypeCreationInfoIsNotNull()
        {
            if (Registration.TypeCreationInfo == null)
                throw new InvalidOperationException($"Cannot instantiate type {Registration.TypeKey.GetFullRegistrationName()} because no Type Creation Info was registered for it.");
        }

        public object GetPerResolveInstance()
        {
            object perResolveInstance;
            if (_resolveScope == null)
            {
                _resolveScope = new Dictionary<TypeKey, object>();
                perResolveInstance = CreateInstance();
                _resolveScope.Add(Registration.TypeKey, perResolveInstance);
                return perResolveInstance;
            }

            if (_resolveScope.TryGetValue(Registration.TypeKey, out perResolveInstance))
                return perResolveInstance;

            perResolveInstance = CreateInstance();
            _resolveScope.Add(Registration.TypeKey, perResolveInstance);
            return perResolveInstance;
        }
    }
}