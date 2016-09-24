using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ChildRegistrationNameOptions<TRegistrationOptions> : IChildRegistrationNameOptions<TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForTypes<TRegistrationOptions>
    {
        private readonly TRegistrationOptions _registrationOptions;
        private readonly ISetChildValueRegistrationName _targetDependency;

        public ChildRegistrationNameOptions(TRegistrationOptions registrationOptions, ISetChildValueRegistrationName targetDependency)
        {
            registrationOptions.MustNotBeNull(nameof(registrationOptions));
            targetDependency.MustNotBeNull(nameof(targetDependency));

            _registrationOptions = registrationOptions;
            _targetDependency = targetDependency;
        }

        public TRegistrationOptions WithName(string childValueRegistrationName)
        {
            _targetDependency.ChildValueRegistrationName = childValueRegistrationName;
            return _registrationOptions;
        }
    }
}