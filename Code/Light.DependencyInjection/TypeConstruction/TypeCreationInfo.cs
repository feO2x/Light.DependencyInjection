using System;
using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeCreationInfo
    {
        public readonly InstanceInjectionInfo InstanceInjectionInfo;
        public readonly TypeCreationKind Kind;
        public readonly Type TargetType;
        public readonly TypeInstantiationInfo TypeInstantiationInfo;

        private TypeCreationInfo(Type targetType, TypeInstantiationInfo typeInstantiationInfo, InstanceInjectionInfo instanceInjectionInfo, TypeCreationKind kind)
        {
            TargetType = targetType;
            TypeInstantiationInfo = typeInstantiationInfo;
            InstanceInjectionInfo = instanceInjectionInfo;
            Kind = kind;
        }

        public object CreateInstance(DiContainer container)
        {
            var instance = TypeInstantiationInfo.Instantiate(container);
            InstanceInjectionInfo?.PerformInjections(instance, container);

            return instance;
        }

        public static TypeCreationInfo FromTypeInstantiatedByDiContainer(TypeInstantiationInfo typeInstantiationInfo, InstanceInjectionInfo instanceInjectionInfo)
        {
            typeInstantiationInfo.MustNotBeNull(nameof(typeInstantiationInfo));

            return new TypeCreationInfo(typeInstantiationInfo.TargetType, typeInstantiationInfo, instanceInjectionInfo, TypeCreationKind.InstantiatedByDiContainer);
        }

        public static TypeCreationInfo FromExternalInstance(object instance)
        {
            instance.MustNotBeNull(nameof(instance));

            return new TypeCreationInfo(instance.GetType(), null, null, TypeCreationKind.ExternalInstance);
        }

        public static TypeCreationInfo FromUnboundGenericType(TypeInstantiationInfo typeInstantiationInfo, InstanceInjectionInfo instanceInjectionInfo)
        {
            typeInstantiationInfo.MustNotBeNull(nameof(typeInstantiationInfo));
            CheckUnboundType(typeInstantiationInfo.TargetType);

            return new TypeCreationInfo(typeInstantiationInfo.TargetType, typeInstantiationInfo, instanceInjectionInfo, TypeCreationKind.UnboundGenericTypeTemplate);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckUnboundType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            if (type.GetTypeInfo().IsGenericTypeDefinition == false)
                throw new TypeRegistrationException($"The type \"{type}\" is no unbound generic type.", type);
        }
    }
}