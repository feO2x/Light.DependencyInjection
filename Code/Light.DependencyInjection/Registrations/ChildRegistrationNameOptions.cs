using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ChildRegistrationNameOptions<T> : IChildRegistrationNameOptions<T>
    {
        private readonly IRegistrationOptions<T> _registrationOptions;
        private readonly ISetChildValueRegistrationName _targetDependency;

        public ChildRegistrationNameOptions(IRegistrationOptions<T> registrationOptions, ISetChildValueRegistrationName targetDependency)
        {
            registrationOptions.MustNotBeNull(nameof(registrationOptions));

            _registrationOptions = registrationOptions;
            _targetDependency = targetDependency;
        }

        public IRegistrationOptions<T> WithName(string childValueRegistrationName)
        {
            _targetDependency.ChildValueRegistrationName = childValueRegistrationName;
            return _registrationOptions;
        }
    }
}