using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    /// <summary>
    ///     Represents an excpetion that indicates an error during type registration.
    /// </summary>
    public class RegistrationException : ArgumentException
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The exception that led to this one.</param>
        public RegistrationException(string message, Exception innerException = null)
            : base(message.MustNotBeNullOrWhiteSpace(nameof(message)), innerException) { }
    }
}