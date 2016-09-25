using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class GenericTypeDefinitionRegistration : BaseRegistration
    {
        public GenericTypeDefinitionRegistration(TypeCreationInfo typeCreationInfo,
                                                 ILifetime lifetime,
                                                 bool isTrackingDisposable = true)
            : base(typeCreationInfo.TypeKey, lifetime, typeCreationInfo, isTrackingDisposable)
        {
            TargetTypeInfo.MustBeGenericTypeDefinition();
        }

        public Registration BindToClosedGenericType(Type closedGenericType)
        {
            closedGenericType.MustBeClosedConstructedVariantOf(TypeKey.Type);

            return new Registration(new TypeKey(closedGenericType, TypeKey.RegistrationName),
                                    _lifetime.ProvideInstanceForResolvedGenericType(),
                                    TypeCreationInfo.CloneForClosedConstructedGenericType(closedGenericType, closedGenericType.GetTypeInfo()));
        }
    }
}