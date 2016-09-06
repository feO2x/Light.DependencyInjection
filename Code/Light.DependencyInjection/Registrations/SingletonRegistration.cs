using System;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class SingletonRegistration : Registration, ISingletonRegistration
    {
        private volatile bool _isCreatingNewInstanceOnNextResolve = true;

        public SingletonRegistration(TypeCreationInfo typeCreationInfo, bool isContainerTrackingDisposables)
            : base(typeCreationInfo.TypeKey, isContainerTrackingDisposables, typeCreationInfo) { }

        public override bool IsCreatingNewInstanceOnNextResolve => _isCreatingNewInstanceOnNextResolve;

        protected override object CreateInstanceInternal(DiContainer container, ParameterOverrides parameterOverrides)
        {
            object singleton;
            if (container.Scope.GetOrAddSingleton(TypeKey,
                                                  () => TypeCreationInfo.CreateInstance(container, parameterOverrides),
                                                  out singleton))
            {
                _isCreatingNewInstanceOnNextResolve = false;
                return singleton;
            }
            throw new ResolveTypeException($"The type {TypeKey.GetFullRegistrationName()} cannot be instantiated because it is registered as a Singleton which has already been instantiated.", TargetType);
        }

        protected override object GetInstanceInternal(DiContainer container)
        {
            var instance = container.Scope.GetOrAddSingleton(TypeKey,
                                                             () => TypeCreationInfo.CreateInstance(container));
            _isCreatingNewInstanceOnNextResolve = false;
            return instance;
        }

        protected override Registration BindGenericTypeDefinition(Type closedConstructedGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new SingletonRegistration(TypeCreationInfo.CloneForClosedConstructedGenericType(closedConstructedGenericType, boundGenericTypeInfo),
                                             IsContainerTrackingDisposable);
        }
    }
}