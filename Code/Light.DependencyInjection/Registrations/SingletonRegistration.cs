using System;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class SingletonRegistration : Registration
    {
        private object _instance;

        public SingletonRegistration(TypeCreationInfo typeCreationInfo, string registrationName = null) 
            : base(new TypeKey(typeCreationInfo.TargetType, registrationName), typeCreationInfo) { }

        protected override object GetInstanceInternal(DiContainer container)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = TypeCreationInfo.CreateInstance(container);
                }
            }
            return _instance;
        }

        protected override Registration BindGenericTypeRegistrationInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new SingletonRegistration(TypeCreationInfo.CloneForBoundGenericType(boundGenericType, boundGenericTypeInfo), TypeKey.RegistrationName);
        }
    }
}