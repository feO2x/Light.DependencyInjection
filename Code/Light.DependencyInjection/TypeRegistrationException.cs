using System;

namespace Light.DependencyInjection
{
    public class TypeRegistrationException : ArgumentException
    {
        public readonly Type TargetType;

        public TypeRegistrationException(string message, Type targetType, Exception innerException = null)
            : base(message, innerException)
        {
            TargetType = targetType;
        }
    }
}