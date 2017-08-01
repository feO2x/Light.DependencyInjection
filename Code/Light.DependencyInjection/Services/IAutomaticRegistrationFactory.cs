using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Services
{
    public interface IAutomaticRegistrationFactory
    {
        Registration CreateDefaultRegistration(TypeKey typeKey, DiContainer container);
    }
}