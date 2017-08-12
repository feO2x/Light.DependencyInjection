using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ConstructorInstantiationInfo : InstantiationInfo
    {
        public readonly ConstructorInfo ConstructorInfo;

        public ConstructorInstantiationInfo(TypeKey typeKey,
                                            ConstructorInfo constructorInfo,
                                            IReadOnlyList<Dependency> instantiationDependencies)
            : base(typeKey, instantiationDependencies)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));
            if (constructorInfo.DeclaringType != typeKey.Type)
                throw new RegistrationException($"The constructor \"{constructorInfo}\" is not a constructor of type \"{typeKey.Type}\", but of \"{constructorInfo.DeclaringType}\".", typeKey.Type);

            instantiationDependencies.VerifyDependencies(constructorInfo.GetParameters(), TypeKey.Type, "constructor");

            ConstructorInfo = constructorInfo;
        }
    }
}