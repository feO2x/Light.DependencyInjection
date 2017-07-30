using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstanceManipulationFactory : IEquatable<InstanceManipulationFactory>
    {
        public readonly IReadOnlyList<DependencyFactory> DependencyFactories;
        public readonly string MemberName;
        public readonly Type TargetType;
        public readonly int HashCode;

        protected InstanceManipulationFactory(Type targetType, string memberName, IReadOnlyList<DependencyFactory> dependencyFactories)
        {
            TargetType = targetType.MustNotBeNull(nameof(targetType));
            MemberName = memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            DependencyFactories = dependencyFactories;
            HashCode = Equality.CreateHashCode(targetType, memberName);
        }

        public abstract InstanceManipulation Create(string registrationName = "");

        public bool Equals(InstanceManipulationFactory other)
        {
            if (other == null) return false;

            return TargetType == other.TargetType && MemberName == other.MemberName;

        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstanceManipulationFactory);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }
    }
}