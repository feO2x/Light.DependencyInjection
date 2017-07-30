using System;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the combination of a Type and string to uniquely identify registrations.
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
        ///     Initializes a new instance of <see cref="TypeKey" />.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> associated with the new type key.</param>
        /// <param name="registrationName">The name of the registration (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> or <paramref name="registrationName" /> is null.</exception>
        public TypeKey(Type type, string registrationName = "")
        {
            Type = type.MustNotBeNull(nameof(type));
            RegistrationName = registrationName.MustNotBeNull(nameof(registrationName));
            HashCode = registrationName.IsNullOrWhiteSpace() ? type.GetHashCode() : Equality.CreateHashCode(type, registrationName);
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
            if (ReferenceEquals(@object, null)) return false;
            return @object is TypeKey && Equals((TypeKey) @object);
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
            return RegistrationName.IsNullOrWhiteSpace() ? Type.ToString() : $"\"{Type}\" with name \"{RegistrationName}\"";
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

        /// <summary>
        ///     Gets the value indicating if this instance was created with the default struct initializer
        ///     (i.e. this instance contains no values).
        /// </summary>
        public bool IsEmpty => ReferenceEquals(Type, null);

        /// <summary>
        ///     Checks if this instance was created with the default struct initializer,
        ///     if yes, an <see cref="ArgumentException" /> is thrown.
        /// </summary>
        /// <param name="parameterName">The name of the parameter (optional).</param>
        public TypeKey MustNotBeEmpty(string parameterName = null)
        {
            if (IsEmpty)
                throw new ArgumentException("The specified type key must not be empty.", parameterName);

            return this;
        }
    }
}