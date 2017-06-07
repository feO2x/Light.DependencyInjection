using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public static class InstantiationDependencyFactoryExtensions
    {
        public static IReadOnlyList<InstantionDependencyFactory> ExtractDependencies(this ConstructorInfo constructorInfo)
        {
            return constructorInfo.ExtractDependencies(constructorInfo.DeclaringType);
        }

        public static IReadOnlyList<InstantionDependencyFactory> ExtractDependencies(this MethodBase instantiationMethod, Type targetType)
        {
            instantiationMethod.MustNotBeNull(nameof(instantiationMethod));
            targetType.MustNotBeNull(nameof(targetType));

            return instantiationMethod.GetParameters()
                                      .Select(parameterInfo => new InstantionDependencyFactory(targetType, parameterInfo))
                                      .ToList();
        }
    }
}