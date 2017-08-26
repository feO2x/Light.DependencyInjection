using System;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DependencyOverrideOptions : IDependencyOverrideOptions
    {
        private readonly Registration _targetRegistration;

        public DependencyOverrideOptions(Registration targetRegistration)
        {
            _targetRegistration = targetRegistration.MustNotBeNull(nameof(targetRegistration));
        }

        public IDependencyOverrideOptions Override<TDependency>(TDependency value)
        {
            return this;
        }

        public DependencyOverrides Build()
        {
            throw new NotImplementedException();
        }
    }
}