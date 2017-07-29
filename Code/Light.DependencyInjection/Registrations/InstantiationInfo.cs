using System.Collections.Generic;

namespace Light.DependencyInjection.Registrations
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