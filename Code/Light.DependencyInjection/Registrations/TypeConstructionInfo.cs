using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TypeConstructionInfo
    {
        public readonly IReadOnlyList<InstanceManipulation> InstanceManipulations;
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeConstructionInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, IReadOnlyList<InstanceManipulation> instanceManipulations = null)
        {
            TypeKey = typeKey.MustNotBeEmpty();
            InstantiationInfo = instantiationInfo.MustNotBeNull();
            InstanceManipulations = instanceManipulations;
        }
    }
}