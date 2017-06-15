using System;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration : IEquatable<Registration>
    {
        public readonly Lifetime LifeTime;
        public readonly TypeConstructionInfo TypeConstructionInfo;
        public readonly TypeKey TypeKey;

        public Registration(TypeKey typeKey, Lifetime lifeTime, TypeConstructionInfo typeConstructionInfo)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
            LifeTime = lifeTime.MustNotBeNull(nameof(lifeTime));
            if (lifeTime.IsCreatingNewInstances == false) return;

            TypeConstructionInfo = typeConstructionInfo.MustNotBeNull(message: "The Type Construction Info must not be null when the Lifetime of the registration is able to create new instances of the target type.");
            typeConstructionInfo.TypeKey.MustBe(typeKey, message: "The Type Key of the Type Construction Info is not equal to the Type Key of the registration.");
        }

        public Type TargetType => TypeKey.Type;
        public string RegistrationName => TypeKey.RegistrationName;

        public bool Equals(Registration other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (ReferenceEquals(null, other)) return false;

            return TypeKey == other.TypeKey;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Registration);
        }

        public override int GetHashCode()
        {
            return TypeKey.GetHashCode();
        }

        public static bool operator ==(Registration first, Registration second)
        {
            if (ReferenceEquals(first, second)) return true;
            return !ReferenceEquals(first, null) && first.Equals(second);
        }

        public static bool operator !=(Registration first, Registration second)
        {
            return !(first == second);
        }
    }
}