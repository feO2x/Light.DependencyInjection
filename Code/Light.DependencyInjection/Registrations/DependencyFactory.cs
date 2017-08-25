using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class DependencyFactory : IDependencyOptions
    {
        public readonly Type DependencyType;
        public readonly string Name;
        private bool? _resolveAll;
        private string _targetRegistrationName = "";

        public DependencyFactory(string name, Type dependencyType)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            DependencyType = dependencyType.MustNotBeNull(nameof(dependencyType));
        }

        public string TargetRegistrationName => _targetRegistrationName;

        public IDependencyOptions WithTargetRegistrationName(string registrationName)
        {
            _targetRegistrationName = registrationName;
            return this;
        }

        public IDependencyOptions SetResolveAllTo(bool? resolveAll)
        {
            _resolveAll = resolveAll;
            return this;
        }

        public Dependency Create()
        {
            return new Dependency(Name, DependencyType, TargetRegistrationName, _resolveAll);
        }
    }
}