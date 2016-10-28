using System;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the options for setting a target registration name with a fluent API.
    /// </summary>
    /// <typeparam name="TRegistrationOptions">The registration options returned by the fluent API.</typeparam>
    public sealed class ChildRegistrationNameOptions<TRegistrationOptions> : IChildRegistrationNameOptions<TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForType<TRegistrationOptions>
    {
        private readonly TRegistrationOptions _registrationOptions;
        private readonly ISetTargetRegistrationName _targetDependency;

        /// <summary>
        ///     Initializes a new instance of <see cref="ChildRegistrationNameOptions{TRegistrationOptions}" />
        /// </summary>
        /// <param name="registrationOptions">The registration options that should be returned by the fluent API.</param>
        /// <param name="targetDependency">The dependency whose target registration name should be changed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrationOptions" /> or <paramref name="targetDependency" /> is null.</exception>
        public ChildRegistrationNameOptions(TRegistrationOptions registrationOptions, ISetTargetRegistrationName targetDependency)
        {
            registrationOptions.MustNotBeNull(nameof(registrationOptions));
            targetDependency.MustNotBeNull(nameof(targetDependency));

            _registrationOptions = registrationOptions;
            _targetDependency = targetDependency;
        }

        /// <inheritdoc />
        public TRegistrationOptions WithName(string childValueRegistrationName)
        {
            _targetDependency.TargetRegistrationName = childValueRegistrationName;
            return _registrationOptions;
        }
    }
}