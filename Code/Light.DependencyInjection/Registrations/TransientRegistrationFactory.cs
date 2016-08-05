using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistrationFactory : IDefaultRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeInstantiationInfo typeInstantiationInfo)
        {
            return new TransientRegistration(typeInstantiationInfo);
        }
    }
}