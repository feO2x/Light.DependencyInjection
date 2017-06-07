using System.Collections.Generic;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ConstructorInstantiationInfo : InstantiationInfo
    {
        public readonly ConstructorInfo ConstructorInfo;

        public ConstructorInstantiationInfo(TypeKey typeKey,
                                            ConstructorInfo constructorInfo,
                                            IReadOnlyList<InstantiationDependency> instantiationDependencies)
            : base(typeKey, instantiationDependencies)
        {
            ConstructorInfo = constructorInfo.MustNotBeNull();
        }
    }
}