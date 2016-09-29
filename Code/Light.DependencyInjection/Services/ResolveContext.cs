using System;
using System.Collections.Generic;
using System.Diagnostics;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public struct ResolveContext
    {
        public readonly DiContainer Container;
        public readonly Registration Registration;
        public readonly ParameterOverrides? ParameterOverrides;
        private Lazy<Dictionary<TypeKey, object>> _lazyResolveScope;

        public ResolveContext(DiContainer container,
                              Registration registration,
                              Lazy<Dictionary<TypeKey, object>> lazyResolveScope,
                              ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));
            registration.MustNotBeNull(nameof(registration));
            lazyResolveScope.MustNotBeNull(nameof(lazyResolveScope));

            Container = container;
            Registration = registration;
            _lazyResolveScope = lazyResolveScope;
            ParameterOverrides = parameterOverrides;
        }

        public object CreateInstance()
        {
            EnsureTypeCreationInfoIsNotNull();

            return Registration.TypeCreationInfo.CreateInstance(CreationContext.FromResolveContext(this, _lazyResolveScope));
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
            if (_lazyResolveScope.IsValueCreated == false)
            {
                perResolveInstance = CreateInstance();
                _lazyResolveScope.Value.Add(Registration.TypeKey, perResolveInstance);
                return perResolveInstance;
            }

            if (_lazyResolveScope.Value.TryGetValue(Registration.TypeKey, out perResolveInstance))
                return perResolveInstance;

            perResolveInstance = CreateInstance();
            _lazyResolveScope.Value.Add(Registration.TypeKey, perResolveInstance);
            return perResolveInstance;
        }
    }
}