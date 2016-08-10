using System;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class Registration : IEquatable<Registration>
    {
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly TypeKey TypeKey;

        protected Registration(TypeCreationInfo typeCreationInfo, string registrationName)
        {
            typeCreationInfo.MustNotBeNull(nameof(typeCreationInfo));

            TypeCreationInfo = typeCreationInfo;
            TypeKey = new TypeKey(typeCreationInfo.TargetType, registrationName);
        }

        public bool IsDefaultRegistration => string.IsNullOrEmpty(TypeKey.RegistrationName);
        public Type TargetType => TypeKey.Type;
        public string Name => TypeKey.RegistrationName;

        public abstract object GetInstance(DiContainer container);
        public bool Equals(Registration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Registration);
        }

        public override int GetHashCode()
        {
            return TypeKey.HashCode;
        }

        public static bool operator ==(Registration first, Registration second)
        {
            return first.EqualsWithHashCode(second);
        }

        public static bool operator !=(Registration first, Registration second)
        {
            return !(first == second);
        }
    }
}