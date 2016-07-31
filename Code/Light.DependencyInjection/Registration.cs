using System;

namespace Light.DependencyInjection
{
    public abstract class Registration
    {
        public readonly string RegistrationName;
        public readonly Type TargetType;
        public readonly TypeInstantiationInfo TypeInstantiationInfo;

        protected Registration(TypeInstantiationInfo typeInstantiationInfo, string registrationName)
        {
            TypeInstantiationInfo = typeInstantiationInfo;
            TargetType = typeInstantiationInfo.TargetType;
            RegistrationName = registrationName;
        }

        public bool IsDefaultRegistration => string.IsNullOrEmpty(RegistrationName);

        public abstract object GetInstance(DiContainer container);
    }
}