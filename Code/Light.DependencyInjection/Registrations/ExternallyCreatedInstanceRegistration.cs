using System;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ExternallyCreatedInstanceRegistration : Registration
    {
        public readonly object Instance;

        public ExternallyCreatedInstanceRegistration(object instance, string registrationName = null) : base(new TypeKey(instance.GetType(), registrationName), false)
        {
            Instance = instance;
        }

        protected override object CreateInstanceInternal(DiContainer container, ParameterOverrides parameterOverrides)
        {
            throw new ResolveTypeException($"The type {TypeKey.GetFullRegistrationName()} cannot be instantiated because it was passed as a reference to the DI container.", TargetType);
        }

        protected override object GetInstanceInternal(DiContainer container)
        {
            return container.Scope.GetOrAddSingleton(TypeKey, ReturnInstance);
        }

        private object ReturnInstance()
        {
            return Instance;
        }

        protected override Registration BindGenericTypeDefinition(Type closedConstructedGenericType, TypeInfo boundGenericTypeInfo)
        {
            throw new NotSupportedException($"The {nameof(ExternallyCreatedInstanceRegistration)} does not support generic type definitions (an instance passed in is always resolved if it has a generic type).");
        }
    }
}