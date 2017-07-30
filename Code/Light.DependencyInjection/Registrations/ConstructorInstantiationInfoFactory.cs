using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ConstructorInstantiationInfoFactory : InstantiationInfoFactory
    {
        public readonly ConstructorInfo ConstructorInfo;

        public ConstructorInstantiationInfoFactory(ConstructorInfo constructorInfo)
            : base(constructorInfo.MustNotBeNull(nameof(constructorInfo)).DeclaringType, constructorInfo.ExtractDependencies())
        {
            ConstructorInfo = constructorInfo;
        }

        public override InstantiationInfo Create(string registrationName = "")
        {
            return new ConstructorInstantiationInfo(new TypeKey(TargetType, registrationName), ConstructorInfo, InstantiationDependencyFactories.CreateDependencies());
        }
    }
}