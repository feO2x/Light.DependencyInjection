using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class ResolveTypeException : ArgumentException
    {
        public readonly Type TargetType;

        public ResolveTypeException(string message, Type targetType) : base(message)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
        }
    }
}