using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TypeConstructionInfo
    {
        public readonly IReadOnlyList<InstanceManipulation> InstanceManipulations;
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeConstructionInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, IReadOnlyList<InstanceManipulation> instanceManipulations = null)
        {
            InstantiationInfo = instantiationInfo.MustNotBeNull(nameof(instantiationInfo));
            TypeKey = typeKey.MustBe(instantiationInfo.TypeKey, parameterName: nameof(typeKey));
            instanceManipulations?.ForEach(instanceManipulation => instanceManipulation.TypeKey.MustBe(typeKey));
            InstanceManipulations = instanceManipulations;
        }
    }
}