using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public interface IDefaultRegistrationFactory
    {
        Registration CreateDefaultRegistration(TypeCreationInfo typeCreationInfo);
    }
}