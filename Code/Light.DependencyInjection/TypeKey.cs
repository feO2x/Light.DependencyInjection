using System;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection
{
    public struct TypeKey : IEquatable<TypeKey>
    {
        public readonly Type Type;
        public readonly string OptionalName;
        private readonly int _hashCode;

        public TypeKey(Type type, string optionalName = null)
        {
            type.MustNotBeNull(nameof(type));

            Type = type;
            OptionalName = optionalName;
            _hashCode = optionalName == null ? type.GetHashCode() : Equality.CreateHashCode(type, optionalName);
        }

        public bool Equals(TypeKey other)
        {
            return Type == other.Type &&
                   OptionalName == other.OptionalName;
        }

        public override bool Equals(object obj)
        {
            try
            {
                return Equals((TypeKey) obj);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}