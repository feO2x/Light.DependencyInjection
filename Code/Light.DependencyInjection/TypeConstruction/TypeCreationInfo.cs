using System;
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

        public static TypeCreationInfo FromTypeInstantiatedByDiContainer(Type targetType, TypeInstantiationInfo typeInstantiationInfo, InstanceInjectionInfo instanceInjectionInfo)
        {
            targetType.MustNotBeNull(nameof(targetType));
            typeInstantiationInfo.MustNotBeNull(nameof(targetType));

            return new TypeCreationInfo(targetType, typeInstantiationInfo, instanceInjectionInfo, TypeCreationKind.CreatedByDiContainer);
        }

        public static TypeCreationInfo FromExternalInstance(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            return new TypeCreationInfo(targetType, null, null, TypeCreationKind.CreatedExternally);
        }
    }
}