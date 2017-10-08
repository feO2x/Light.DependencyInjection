using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstanceManipulation : IDependencyInfo, IEquatable<InstanceManipulation>
    {
        public readonly IReadOnlyList<Dependency> Dependencies;
        public readonly string MemberName;
        public readonly TypeKey TypeKey;

        protected InstanceManipulation(TypeKey typeKey, string memberName, IReadOnlyList<Dependency> dependencies)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
            MemberName = memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            Dependencies = dependencies;
        }

        IReadOnlyList<Dependency> IDependencyInfo.Dependencies => Dependencies;

        public bool Equals(InstanceManipulation other)
        {
            if (other == null) return false;

            return TypeKey == other.TypeKey && MemberName == other.MemberName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstanceManipulation);
        }

        public override int GetHashCode()
        {
            return Equality.CreateHashCode(TypeKey, MemberName);
        }
    }
}