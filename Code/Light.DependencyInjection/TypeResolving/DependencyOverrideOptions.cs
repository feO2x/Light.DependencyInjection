using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DependencyOverrideOptions : IDependencyOverrideOptions
    {
        private readonly Registration _targetRegistration;
        private Dictionary<Dependency, object> _overriddenDependencies;

        public DependencyOverrideOptions(Registration targetRegistration)
        {
            _targetRegistration = targetRegistration.MustNotBeNull(nameof(targetRegistration));
            if (targetRegistration.TypeConstructionInfo == null)
                throw new ResolveException($"You cannot override dependencies of {_targetRegistration.TypeKey} because the corresponding registration is configured to not create instances.");
        }

        private Dictionary<Dependency, object> OverriddenDependencies => _overriddenDependencies ?? (_overriddenDependencies = new Dictionary<Dependency, object>());

        public Registration TargetRegistration => _targetRegistration;

        public IDependencyOverrideOptions Override<TDependency>(TDependency value)
        {
            var dependencyType = typeof(TDependency);
            if (_targetRegistration.TypeConstructionInfo.AllDependencies.IsNullOrEmpty())
                throw new ResolveException($"You cannot override dependencies because the registration {_targetRegistration} has no dependencies configured.");

            var allRegisteredDependencies = _targetRegistration.TypeConstructionInfo.AllDependencies;
            Dependency targetDependency = null;
            for (var i = 0; i < allRegisteredDependencies.Count; i++)
            {
                var currentDependency = allRegisteredDependencies[i];
                if (currentDependency.TargetType != dependencyType)
                    continue;

                if (targetDependency != null)
                    throw new ResolveException($"There are several dependencies of type \"{dependencyType}\" on the target type {_targetRegistration.TypeKey} so that the dependency to be overridden cannot be selected uniquely. Please use the overload of {nameof(Override)} that accepts a name to uniquely identify the target dependency.");

                targetDependency = currentDependency;
            }

            if (targetDependency == null)
                throw new ResolveException($"There is no dependency of type \"{dependencyType}\" configured on the target type {_targetRegistration.TargetType}.");

            OverriddenDependencies.Add(targetDependency, value);
            return this;
        }

        public DependencyOverrides Build()
        {
            return new DependencyOverrides(_overriddenDependencies);
        }
    }
}