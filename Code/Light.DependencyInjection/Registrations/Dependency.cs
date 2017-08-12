using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Dependency
    {
        public readonly string Name;
        public readonly Type DependencyType;
        public readonly string TargetRegistrationName;

        public Dependency(string name, Type dependencyType, string targetRegistrationName)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            DependencyType = dependencyType.MustNotBeNull(nameof(dependencyType));
            TargetRegistrationName = targetRegistrationName.MustNotBeNull(nameof(targetRegistrationName));
        }

        public override string ToString()
        {
            var registrationNameText = TargetRegistrationName == string.Empty ? string.Empty : $" (\"{TargetRegistrationName}\")";
            return $"{DependencyType} {Name}{registrationNameText}";
        }
    }
}