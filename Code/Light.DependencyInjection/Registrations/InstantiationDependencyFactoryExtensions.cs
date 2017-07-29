using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;
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

        public static IReadOnlyList<InstantiationDependency> CreateInstantiationDependencies(this IReadOnlyList<InstantionDependencyFactory> instantiationDependencies, string registrationName)
        {
            if (instantiationDependencies.Count == 0)
                return null;

            var array = new InstantiationDependency[instantiationDependencies.Count];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = instantiationDependencies[i].Create(registrationName);
            }

            return array;
        }
    }
}