using System;
using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ExternallyCreatedInstanceRegistration : Registration
    {
        public readonly object Instance;

        public ExternallyCreatedInstanceRegistration(object instance, string registrationName = null) : base(new TypeKey(instance.GetType(), registrationName))
        {
            Instance = instance;
        }

        protected override object GetInstanceInternal(DiContainer container)
        {
            return Instance;
        }

        protected override Registration BindGenericTypeRegistrationInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            throw new NotSupportedException($"The {nameof(ExternallyCreatedInstanceRegistration)} does not support generic unbound types (an instance passed in is always resolved if it has a generic type).");
        }
    }
}