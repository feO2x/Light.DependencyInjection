using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistrationFactory : IDefaultRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeCreationInfo typeCreationInfo)
        {
            return new TransientRegistration(typeCreationInfo);
        }
    }
}