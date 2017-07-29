using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TypeConstructionInfo
    {
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeConstructionInfo(TypeKey typeKey, InstantiationInfo instantiationInfo)
        {
            TypeKey = typeKey.MustNotBeEmpty();
            InstantiationInfo = instantiationInfo.MustNotBeNull();
        }
    }
}