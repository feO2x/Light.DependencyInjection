using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DependencyOverrideOptions : IDependencyOverrideOptions
    {
        private readonly Registration _targetRegistration;
        private Dictionary<Dependency, object> _overriddenDependencies;
        private Dictionary<TypeKey, object> _overriddenRegistrations;

        public DependencyOverrideOptions(Registration targetRegistration)
        {
            _targetRegistration = targetRegistration.MustNotBeNull(nameof(targetRegistration));
            if (targetRegistration.TypeConstructionInfo == null)
                throw new ResolveException($"You cannot override dependencies of {_targetRegistration.TypeKey} because the corresponding registration is configured to not create instances.");
        }

        private Dictionary<Dependency, object> OverriddenDependencies => _overriddenDependencies ?? (_overriddenDependencies = new Dictionary<Dependency, object>());
        private Dictionary<TypeKey, object> OverriddenRegistrations => _overriddenRegistrations ?? (_overriddenRegistrations = new Dictionary<TypeKey, object>());

        public Registration TargetRegistration => _targetRegistration;

        public IDependencyOverrideOptions OverrideDependency<TDependency>(TDependency value)
        {
            return OverrideDependency(typeof(TDependency), value);
        }

        public IDependencyOverrideOptions OverrideDependency<TDependency>(string dependencyName, TDependency value, StringComparison nameComparisonType = StringComparison.CurrentCulture)
        {
            return OverrideDependency(dependencyName, (object)value, nameComparisonType);
        }

        public IDependencyOverrideOptions OverrideDependency(Type dependencyType, object dependencyValue)
        {
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
                    throw new ResolveException($"There are several dependencies of type \"{dependencyType}\" on the target type {_targetRegistration} so that the dependency to be overridden cannot be selected uniquely. Please use the overload of {nameof(OverrideDependency)} that accepts a name to uniquely identify the target dependency.");

                targetDependency = currentDependency;
            }

            if (targetDependency == null)
                throw new ResolveException($"There is no dependency of type \"{dependencyType}\" configured on the target type {_targetRegistration}.");

            OverriddenDependencies.Add(targetDependency, dependencyValue);
            return this;
        }

        public IDependencyOverrideOptions OverrideDependency(string dependencyName, object dependencyValue, StringComparison nameComparisonType = StringComparison.CurrentCulture)
        {
            dependencyName.MustNotBeNullOrEmpty(nameof(dependencyName));
            if (_targetRegistration.TypeConstructionInfo.AllDependencies.IsNullOrEmpty())
                throw new ResolveException($"You cannot override dependencies because the registration {_targetRegistration} has no dependencies configured.");

            var allRegisteredDependencies = _targetRegistration.TypeConstructionInfo.AllDependencies;
            for (var i = 0; i < allRegisteredDependencies.Count; i++)
            {
                var currentDependency = allRegisteredDependencies[i];
                if (currentDependency.Name.Equals(dependencyName, nameComparisonType) == false)
                    continue;

                OverriddenDependencies.Add(currentDependency, dependencyValue);
                return this;
            }

            throw new ResolveException($"There is no dependency with name \"{dependencyName}\" configured on the target type {_targetRegistration}.");
        }

        public IDependencyOverrideOptions OverrideRegistration<T>(T value, string registrationName = "")
        {
            return OverrideRegistration(typeof(T), value, registrationName);
        }

        public IDependencyOverrideOptions OverrideRegistration(Type type, object value, string registrationName = "")
        {
            var typeKey = new TypeKey(type, registrationName);
            var overriddenDependencies = OverriddenRegistrations;
            if (overriddenDependencies.ContainsKey(typeKey))
                overriddenDependencies[typeKey] = value;
            else
                overriddenDependencies.Add(typeKey, value);

            return this;
        }

        public DependencyOverrides Build()
        {
            return new DependencyOverrides(_overriddenDependencies, _overriddenRegistrations);
        }
    }
}