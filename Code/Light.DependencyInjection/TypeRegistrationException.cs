using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    /// <summary>
    ///     Represents an excpetion that indicates an error during type registration.
    /// </summary>
    public class TypeRegistrationException : ArgumentException
    {
        /// <summary>
        ///     The type that could not be registered.
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        ///     Initializes a new instance of <see cref="TypeRegistrationException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="targetType">The type that could not be registered properly.</param>
        /// <param name="innerException">The exception that led to this one.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> is null.</exception>
        public TypeRegistrationException(string message, Type targetType, Exception innerException = null)
            : base(message, innerException)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }
    }
}