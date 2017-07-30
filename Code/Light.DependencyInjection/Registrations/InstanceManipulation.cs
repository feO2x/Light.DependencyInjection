using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstanceManipulation : IEquatable<InstanceManipulation>
    {
        public readonly TypeKey TypeKey;
        public readonly string MemberName;
        public readonly IReadOnlyList<Dependency> Dependencies;

        protected InstanceManipulation(TypeKey typeKey, string memberName, IReadOnlyList<Dependency> dependencies)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
            MemberName = memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            Dependencies = dependencies;
        }

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