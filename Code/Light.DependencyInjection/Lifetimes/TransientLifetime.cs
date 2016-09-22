using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class TransientLifetime : ILifetime
    {
        public object GetInstance(Registration registration, DiContainer container)
        {
            return registration.TypeCreationInfo.CreateInstance(container);
        }
    }
}