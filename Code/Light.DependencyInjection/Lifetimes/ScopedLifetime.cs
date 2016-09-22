using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : ILifetime
    {
        public object GetInstance(Registration registration, DiContainer container)
        {
            return container.Scope.GetOrAddObject(registration.TypeKey,
                                                  () => registration.TypeCreationInfo.CreateInstance(container));
        }
    }
}