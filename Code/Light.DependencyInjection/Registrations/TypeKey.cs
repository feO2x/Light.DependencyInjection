using System;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the combination of a type and string to uniquely identify registrations.
    /// </summary>
    public struct TypeKey : IEquatable<TypeKey>
    {
        /// <summary>
        ///     Gets the type associated with this type key.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     Gets the registration name of this type key.
        /// </summary>
        public readonly string RegistrationName;

        /// <summary>
        ///     Gets the hash code of this type key.
        /// </summary>
        public readonly int HashCode;

        /// <summary>
        ///     Gets the hash code of the type.
        /// </summary>
        public readonly int TypeHashCode;

        /// <summary>
        ///     Initializes a new instance of <see cref="TypeKey" />.
        /// </summary>
        /// <param name="type">The type associated with the new type key.</param>
        /// <param name="registrationName">The name of the registration (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public TypeKey(Type type, string registrationName = null)
        {
            type.MustNotBeNull(nameof(type));

            Type = type;
            RegistrationName = registrationName;
            TypeHashCode = HashCode = type.GetHashCode();
            if (registrationName != null)
                HashCode = Equality.CreateHashCode(type, registrationName);
        }

        /// <summary>
        ///     Checks if the type and registration name of the other instance is the same as the this ones.
        /// </summary>
        /// <param name="other">The other type key instance.</param>
        /// <returns>True if type and registration name are equal on both instances, else false.</returns>
        public bool Equals(TypeKey other)
        {
            return Type == other.Type &&
                   RegistrationName == other.RegistrationName;
        }

        /// <summary>
        ///     Checks if the specified object is a type key and performs the equality check.
        /// </summary>
        /// <param name="object">The object to be compared.</param>
        /// <returns>True if the specified object is a type key with the same type and registration name, else false.</returns>
        public override bool Equals(object @object)
        {
            try
            {
                return Equals((TypeKey) @object);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets the hash code of this type key.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode;
        }

        /// <summary>
        ///     Checks if the two type key instances are equal.
        /// </summary>
        public static bool operator ==(TypeKey first, TypeKey second)
        {
            return first.HashCode == second.HashCode &&
                   first.Equals(second);
        }

        /// <summary>
        ///     Checks if the two type key intances are not equal.
        /// </summary>
        public static bool operator !=(TypeKey first, TypeKey second)
        {
            return !(first == second);
        }

        /// <summary>
        ///     Returns the string representation of this type key.
        /// </summary>
        public override string ToString()
        {
            return $"TypeKey {this.GetFullRegistrationName()}";
        }

        /// <summary>
        ///     Implicitely converts the specified type to a type key.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public static implicit operator TypeKey(Type type)
        {
            return new TypeKey(type);
        }

        /// <summary>
        ///     Implicitely converts the specified type key to a type.
        /// </summary>
        public static implicit operator Type(TypeKey typeKey)
        {
            return typeKey.Type;
        }
    }
}