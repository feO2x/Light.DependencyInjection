using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        ///     Gets the registration that should be instantiated.
        /// </summary>
        public readonly Registration Registration;

        /// <summary>
        ///     Gets the parameter overrides for this resolve call (if present).
        /// </summary>
        public readonly ParameterOverrides? ParameterOverrides;

        private readonly Lazy<Dictionary<TypeKey, object>> _lazyResolveScope;

        /// <summary>
        ///     Initializes a new instance of <see cref="CreationContext" />.
        /// </summary>
        /// <param name="container">The <see cref="DependencyInjectionContainer" /> instance.</param>
        /// <param name="registration">The target registration to be instantiated.</param>
        /// <param name="lazyResolveScope">The scope for this resolve call.</param>
        /// <param name="parameterOverrides">The parameter overrides for this resolve call.</param>
        public CreationContext(DependencyInjectionContainer container,
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
        private static void CheckThatRegistrationDoesNotDescribeGenericTypeDefinition(Registration registration)
        {
            registration.MustNotBeNull(nameof(registration));

            if (registration.IsRegistrationForGenericTypeDefinition)
                throw new ResolveTypeException($"The type {registration.TargetType} cannot be resolved because it describes a generic type definition.", registration.TargetType);
        }

        /// <summary>
        ///     Creates a new <see cref="CreationContext" /> instance from the specified resolve context and registration.
        /// </summary>
        public static CreationContext FromCreationContext(ResolveContext context, Registration registration)
        {
            return new CreationContext(context.Container,
                                       registration,
                                       context.LazyResolveScope,
                                       context.ParameterOverrides);
        }

        /// <summary>
        ///     Instantiates the registration's type using the registration's type creation info.
        /// </summary>
        /// <returns>A new intance of the target type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the registration is not associated with a type creation info.</exception>
        public object CreateInstance()
        {
            EnsureTypeCreationInfoIsNotNull();

            return Registration.TypeCreationInfo.CreateInstance(ResolveContext.FromCreationContext(this, _lazyResolveScope));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureTypeCreationInfoIsNotNull()
        {
            if (Registration.TypeCreationInfo == null)
                throw new InvalidOperationException($"Cannot instantiate type {Registration.TypeKey.GetFullRegistrationName()} because no Type Creation Info was registered for it.");
        }

        /// <summary>
        ///     Tries to retrieve the target type from the resolve scope, an instantiates it if retrieval was not possible.
        /// </summary>
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