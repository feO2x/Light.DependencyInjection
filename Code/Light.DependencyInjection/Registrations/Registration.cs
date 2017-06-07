using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration
    {
        public readonly Lifetime LifeTime;
        public readonly TypeKey TypeKey;
        public readonly TypeConstructionInfo TypeConstructionInfo;

        public Registration(TypeKey typeKey, Lifetime lifeTime, TypeConstructionInfo typeConstructionInfo)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
            LifeTime = lifeTime.MustNotBeNull(nameof(lifeTime));
            if (lifeTime.IsCreatingNewInstances == false) return;

            TypeConstructionInfo = typeConstructionInfo.MustNotBeNull(message: "The Type Construction Info must not be null when the Lifetime of the registration is able to create new instances of the target type.");
            typeConstructionInfo.TypeKey.MustBe(typeKey, message: "The Type Key of the Type Construction Info is not equal to the Type Key of the registration.");
        }
    }
}