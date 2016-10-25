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
        public readonly DependencyInjectionContainer Container;
        public readonly Registration Registration;
        public readonly ParameterOverrides? ParameterOverrides;
        private readonly Lazy<Dictionary<TypeKey, object>> _lazyResolveScope;

        public ResolveContext(DependencyInjectionContainer container,
                              Registration registration,
                              Lazy<Dictionary<TypeKey, object>> lazyResolveScope,
                              ParameterOverrides? parameterOverrides = null)
        {
            container.MustNotBeNull(nameof(container));
            CheckThatRegistrationDoesNotDescribeGenericTypeDefinition(registration);
            lazyResolveScope.MustNotBeNull(nameof(lazyResolveScope));

            Container = container;
            Registration = registration;
            _lazyResolveScope = lazyResolveScope;
            ParameterOverrides = parameterOverrides;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void CheckThatRegistrationDoesNotDescribeGenericTypeDefinition(Registration registration)
        {
            registration.MustNotBeNull(nameof(registration));

            if (registration.IsRegistrationForGenericTypeDefinition)
                throw new ResolveTypeException($"The type {registration.TargetType} cannot be resolved because it describes a generic type definition.", registration.TargetType);
        }

        public static ResolveContext FromCreationContext(CreationContext context, Registration registration)
        {
            return new ResolveContext(context.Container,
                                      registration,
                                      context.LazyResolveScope,
                                      context.ParameterOverrides);
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

        public object GetOrCreatePerResolveInstance()
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