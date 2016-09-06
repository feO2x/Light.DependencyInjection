using System;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class SingletonRegistration : Registration
    {
        public SingletonRegistration(TypeCreationInfo typeCreationInfo)
            : base(typeCreationInfo.TypeKey, true, typeCreationInfo) { }

        protected override object CreateInstanceInternal(DiContainer container, ParameterOverrides parameterOverrides)
        {
            object singleton;
            if (container.Scope.Singletons.TryGetValue(TypeKey, out singleton))
                throw new ResolveTypeException($"The type {TypeKey.GetFullRegistrationName()} cannot be instantiated because it is registered as a Singleton which has already been instantiated.", TargetType);

            if (container.Scope.Singletons.GetOrAdd(TypeKey,
                                                    () => TypeCreationInfo.CreateInstance(container, parameterOverrides),
                                                    out singleton))
                return singleton;
            throw new ResolveTypeException($"The type {TypeKey.GetFullRegistrationName()} cannot be instantiated because it is registered as a Singleton which has already been instantiated.", TargetType);
        }

        protected override object GetInstanceInternal(DiContainer container)
        {
            var instance = container.Scope.Singletons.GetOrAdd(TypeKey,
                                                               () => TypeCreationInfo.CreateInstance(container));
            return instance;
        }

        protected override Registration BindGenericTypeDefinition(Type closedConstructedGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new SingletonRegistration(TypeCreationInfo.CloneForClosedConstructedGenericType(closedConstructedGenericType, boundGenericTypeInfo));
        }
    }
}