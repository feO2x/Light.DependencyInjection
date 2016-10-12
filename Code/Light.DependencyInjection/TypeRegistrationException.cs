using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class TypeRegistrationException : ArgumentException
    {
        public readonly Type TargetType;

        public TypeRegistrationException(string message, Type targetType = null, Exception innerException = null)
            : base(message, innerException)
        {
            TargetType = targetType;
        }
    }
}