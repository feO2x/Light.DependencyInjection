using System;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Dependency : IEquatable<Dependency>
    {
        public readonly string DependencyKind;
        public readonly int HashCode;
        public readonly string Name;
        public readonly bool? ResolveAll;
        public readonly string TargetRegistrationName;
        public readonly Type TargetType;

        public Dependency(string name, Type targetType, string dependencyKind, string targetRegistrationName = "", bool? resolveAll = null)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            TargetType = targetType.MustNotBeNull(nameof(targetType));
            DependencyKind = dependencyKind.MustNotBeNullOrWhiteSpace(nameof(dependencyKind));
            TargetRegistrationName = targetRegistrationName.MustNotBeNull(nameof(targetRegistrationName));
            ResolveAll = resolveAll;
            HashCode = Equality.CreateHashCode(name, targetType, dependencyKind);
        }

        public bool Equals(Dependency other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (ReferenceEquals(other, null)) return false;

            return Name == other.Name &&
                   TargetType == other.TargetType &&
                   DependencyKind == other.DependencyKind;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Dependency);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override string ToString()
        {
            var registrationNameText = TargetRegistrationName == string.Empty ? string.Empty : $" (\"{TargetRegistrationName}\")";
            return $"{TargetType} {Name}{registrationNameText}";
        }

        public static bool operator ==(Dependency x, Dependency y)
        {
            return ReferenceEquals(x, y) || x?.HashCode == y?.HashCode && x.Equals(y);
        }

        public static bool operator !=(Dependency x, Dependency y)
        {
            return !(x == y);
        }
    }
}