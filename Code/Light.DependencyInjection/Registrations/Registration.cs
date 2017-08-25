using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration : IEquatable<Registration>
    {
        public readonly bool IsTrackingDisposables;
        public readonly Lifetime Lifetime;
        public readonly IReadOnlyList<Type> MappedAbstractionTypes;
        public readonly TypeConstructionInfo TypeConstructionInfo;
        public readonly TypeKey TypeKey;

        public Registration(TypeKey typeKey, Lifetime lifeTime, TypeConstructionInfo typeConstructionInfo = null, IReadOnlyList<Type> mappedAbstractionTypes = null, bool isTrackingDisposables = true)
        {
            TypeKey = typeKey.MustBeValidRegistrationTypeKey();
            Lifetime = lifeTime.MustNotBeNull(nameof(lifeTime));
            if (lifeTime.IsCreatingNewInstances)
            {
                if (typeConstructionInfo == null)
                    throw new RegistrationException($"The Type Construction Info for {typeKey} must not be null when the Lifetime \"{lifeTime}\" of the registration is able to create new instances of the target type.");
                if (typeConstructionInfo.TypeKey != typeKey)
                    throw new RegistrationException($"The Type Key {typeConstructionInfo.TypeKey} of the Type Construction Info is not equal to the Type Key {typeKey} of the registration.");
                TypeConstructionInfo = typeConstructionInfo;
            }
            else if (TargetType.IsGenericTypeDefinition())
                throw new RegistrationException($"You cannot register the generic type definition \"{TargetType}\" when the corresponding lifetime does not create instances.");

            if (mappedAbstractionTypes.IsNullOrEmpty() == false)
            {
                // ReSharper disable once PossibleNullReferenceException
                for (var i = 0; i < mappedAbstractionTypes.Count; i++)
                {
                    mappedAbstractionTypes[i].MustBeBaseTypeOf(TargetType);
                }
            }
            MappedAbstractionTypes = mappedAbstractionTypes;
            IsTrackingDisposables = isTrackingDisposables;
        }

        public Type TargetType => TypeKey.Type;
        public string RegistrationName => TypeKey.RegistrationName;
        public bool IsGenericRegistration => TargetType.IsGenericTypeDefinition();
        public bool IsDefaultRegistration => RegistrationName == string.Empty;

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

        public override string ToString()
        {
            return TypeKey.ToString();
        }
    }
}