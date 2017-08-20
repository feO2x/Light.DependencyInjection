using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Dependency
    {
        public readonly Type DependencyType;
        public readonly string Name;
        public readonly bool? ResolveAll;
        public readonly string TargetRegistrationName;

        public Dependency(string name, Type dependencyType, string targetRegistrationName, bool? resolveAll = null)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            DependencyType = dependencyType.MustNotBeNull(nameof(dependencyType));
            TargetRegistrationName = targetRegistrationName.MustNotBeNull(nameof(targetRegistrationName));
            ResolveAll = resolveAll;
        }

        public override string ToString()
        {
            var registrationNameText = TargetRegistrationName == string.Empty ? string.Empty : $" (\"{TargetRegistrationName}\")";
            return $"{DependencyType} {Name}{registrationNameText}";
        }
    }
}