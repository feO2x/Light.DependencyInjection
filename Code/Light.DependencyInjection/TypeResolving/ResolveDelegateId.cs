using System;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public struct ResolveDelegateId : IEquatable<ResolveDelegateId>
    {
        public readonly TypeKey TypeKey;
        public readonly DependencyOverrides Overrides;
        public readonly int HashCode;

        public ResolveDelegateId(TypeKey typeKey, DependencyOverrides overrides = null)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
            Overrides = overrides;
            HashCode = Overrides == null ? TypeKey.HashCode : Equality.CreateHashCode(TypeKey, overrides);
        }

        public bool Equals(ResolveDelegateId other)
        {
            return TypeKey == other.TypeKey &&
                   Overrides == other.Overrides;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            return obj is ResolveDelegateId id && Equals(id);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public static bool operator ==(ResolveDelegateId x, ResolveDelegateId y)
        {
            return x.HashCode == y.HashCode && x.Equals(y);
        }

        public static bool operator !=(ResolveDelegateId x, ResolveDelegateId y)
        {
            return !(x == y);
        }
    }
}