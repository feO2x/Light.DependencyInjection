using System;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection
{
    public abstract class Registration : IEquatable<Registration>
    {
        public readonly TypeInstantiationInfo TypeInstantiationInfo;
        public readonly TypeKey TypeKey;

        protected Registration(TypeInstantiationInfo typeInstantiationInfo, string registrationName)
        {
            TypeInstantiationInfo = typeInstantiationInfo;
            TypeKey = new TypeKey(typeInstantiationInfo.TargetType, registrationName);
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