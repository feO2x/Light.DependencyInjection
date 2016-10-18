using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ChildRegistrationNameOptions<TRegistrationOptions> : IChildRegistrationNameOptions<TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForType<TRegistrationOptions>
    {
        private readonly TRegistrationOptions _registrationOptions;
        private readonly ISetTargetRegistrationName _targetDependency;

        public ChildRegistrationNameOptions(TRegistrationOptions registrationOptions, ISetTargetRegistrationName targetDependency)
        {
            registrationOptions.MustNotBeNull(nameof(registrationOptions));
            targetDependency.MustNotBeNull(nameof(targetDependency));

            _registrationOptions = registrationOptions;
            _targetDependency = targetDependency;
        }

        public TRegistrationOptions WithName(string childValueRegistrationName)
        {
            _targetDependency.TargetRegistrationName = childValueRegistrationName;
            return _registrationOptions;
        }
    }
}