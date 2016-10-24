using System;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public struct TypeKey : IEquatable<TypeKey>
    {
        public readonly Type Type;
        public readonly string RegistrationName;
        public readonly int HashCode;
        public readonly int TypeHashCode;

        public TypeKey(Type type, string registrationName = null)
        {
            type.MustNotBeNull(nameof(type));

            Type = type;
            RegistrationName = registrationName;
            TypeHashCode = HashCode = type.GetHashCode();
            if (registrationName != null)
                HashCode = Equality.CreateHashCode(type, registrationName);
        }

        public bool Equals(TypeKey other)
        {
            return Type == other.Type &&
                   RegistrationName == other.RegistrationName;
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
            return HashCode;
        }

        public static bool operator ==(TypeKey first, TypeKey second)
        {
            return first.HashCode == second.HashCode &&
                   first.Equals(second);
        }

        public static bool operator !=(TypeKey first, TypeKey second)
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return $"TypeKey {this.GetFullRegistrationName()}";
        }

        public static implicit operator TypeKey(Type type)
        {
            return new TypeKey(type);
        }

        public static implicit operator Type(TypeKey typeKey)
        {
            return typeKey.Type;
        }
    }
}