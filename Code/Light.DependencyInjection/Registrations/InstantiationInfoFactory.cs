using System;
using System.Collections.Generic;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstantiationInfoFactory
    {
        public readonly IReadOnlyList<InstantionDependencyFactory> InstantiationDependencies;
        public readonly Type TargetType;

        protected InstantiationInfoFactory(Type targetType, IReadOnlyList<InstantionDependencyFactory> instantiationDependencies)
        {
            InstantiationDependencies = instantiationDependencies.MustNotBeNull();
            TargetType = targetType.MustNotBeNull();
        }

        public abstract InstantiationInfo Create(string registrationName = "");

        protected IReadOnlyList<InstantiationDependency> CreateInstantiationDependencies(string registrationName)
        {
            if (InstantiationDependencies.Count == 0)
                return null;

            var array = new InstantiationDependency[InstantiationDependencies.Count];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = InstantiationDependencies[i].Create(registrationName);
            }

            return array;
        }
    }
}