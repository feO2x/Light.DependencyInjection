using System;
using System.Collections.Generic;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstantiationInfoFactory
    {
        public readonly IReadOnlyList<InstantionDependencyFactory> InstantiationDependencyFactories;
        public readonly Type TargetType;

        protected InstantiationInfoFactory(Type targetType, IReadOnlyList<InstantionDependencyFactory> instantiationDependencyFactories)
        {
            TargetType = targetType.MustNotBeNull();
            InstantiationDependencyFactories = instantiationDependencyFactories.MustNotBeNull();
        }

        public abstract InstantiationInfo Create(string registrationName = "");
    }
}