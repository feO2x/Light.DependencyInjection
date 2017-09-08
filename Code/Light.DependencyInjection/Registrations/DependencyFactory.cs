using System;

namespace Light.DependencyInjection.Registrations
{
    public sealed class DependencyFactory : IDependencyOptions
    {
        public readonly string DependencyType;
        public readonly string Name;
        public readonly Type TargetType;
        private bool? _resolveAll;
        private string _targetRegistrationName = "";

        public DependencyFactory(string name, Type targetType, string dependencyType)
        {
            Name = name;
            TargetType = targetType;
            DependencyType = dependencyType;
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
            return new Dependency(Name, TargetType, DependencyType, TargetRegistrationName, _resolveAll);
        }
    }
}