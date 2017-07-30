using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class InstantiationInfoFactory
    {
        public readonly IReadOnlyList<DependencyFactory> InstantiationDependencyFactories;
        public readonly Type TargetType;

        protected InstantiationInfoFactory(Type targetType, IReadOnlyList<DependencyFactory> instantiationDependencyFactories)
        {
            TargetType = targetType.MustNotBeNull();
            InstantiationDependencyFactories = instantiationDependencyFactories.MustNotBeNull();
        }

        public abstract InstantiationInfo Create(string registrationName = "");
    }
}