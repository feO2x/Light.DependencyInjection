using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : ILifetime
    {
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        public object GetInstance(Registration registration, DiContainer container)
        {
            return container.Scope.GetOrAddInstance(registration.TypeKey,
                                                    () => registration.TypeCreationInfo.CreateInstance(container, registration.IsTrackingDisposables));
        }

        public object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides)
        {
            object instance;
            if (container.Scope.GetOrAddInstance(registration.TypeKey,
                                                 () => registration.TypeCreationInfo.CreateInstance(container, parameterOverrides, registration.IsTrackingDisposables),
                                                 out instance) == false)
                throw new ResolveTypeException($"The type {registration.TypeKey.GetFullRegistrationName()} is registered with a scoped lifetime and already instantiated. Thus CreateInstance cannot be called successfully.", registration.TargetType);

            return instance;
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return this;
        }
    }
}