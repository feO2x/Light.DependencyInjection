using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class TransientLifetime : ILifetime
    {
        public static readonly TransientLifetime Instance = new TransientLifetime();

        public object GetInstance(Registration registration, DiContainer container)
        {
            return registration.TypeCreationInfo.CreateInstance(container, registration.IsTrackingDisposables);
        }

        public object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides)
        {
            return registration.TypeCreationInfo.CreateInstance(container, parameterOverrides, registration.IsTrackingDisposables);
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return this;
        }
    }
}