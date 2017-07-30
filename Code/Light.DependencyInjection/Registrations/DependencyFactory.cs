using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class DependencyFactory
    {
        private readonly Type _dependencyType;
        private readonly string _name;
        private string _targetRegistrationName = "";

        public DependencyFactory(string name, Type dependencyType)
        {
            _name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            _dependencyType = dependencyType.MustNotBeNull(nameof(dependencyType));
        }

        public string TargetRegistrationName
        {
            get => _targetRegistrationName;
            set => _targetRegistrationName = value.MustNotBeNull();
        }

        public Dependency Create()
        {
            return new Dependency(_name, _dependencyType, TargetRegistrationName);
        }
    }
}