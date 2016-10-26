using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class ResolveTypeException : ArgumentException
    {
        public readonly Type TargetType;

        public ResolveTypeException(string message, Type targetType, Exception innerException = null) : base(message, innerException)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }
    }
}