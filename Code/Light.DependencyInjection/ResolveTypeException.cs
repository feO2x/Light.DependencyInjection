using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    /// <summary>
    ///     Represents an exception indicating that a type could not be resolved.
    /// </summary>
    public class ResolveTypeException : ArgumentException
    {
        /// <summary>
        ///     The type that could not be resolved.
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        ///     Initializes a new instance of <see cref="ResolveTypeException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="targetType">The type that could not be resolved.</param>
        /// <param name="innerException">The inner exception that led to this one.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> is null.</exception>
        public ResolveTypeException(string message, Type targetType, Exception innerException = null) : base(message, innerException)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }
    }
}