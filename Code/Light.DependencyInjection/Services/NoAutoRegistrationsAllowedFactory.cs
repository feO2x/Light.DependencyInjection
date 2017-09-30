using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Services
{
    public sealed class NoAutoRegistrationsAllowedFactory : IAutomaticRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeKey typeKey, DiContainer container)
        {
            throw new ResolveException($"There is no registration for {typeKey} and the container is configured to not perform automatic registrations.");
        }
    }
}