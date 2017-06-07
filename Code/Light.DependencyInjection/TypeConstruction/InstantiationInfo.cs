using System.Collections.Generic;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstantiationInfo
    {
        public readonly IReadOnlyList<InstantiationDependency> InstantiationDependencies;
        public readonly TypeKey TypeKey;

        protected InstantiationInfo(TypeKey typeKey, IReadOnlyList<InstantiationDependency> instantiationDependencies)
        {
            TypeKey = typeKey.MustNotBeEmpty();
            InstantiationDependencies = instantiationDependencies;
        }
    }
}