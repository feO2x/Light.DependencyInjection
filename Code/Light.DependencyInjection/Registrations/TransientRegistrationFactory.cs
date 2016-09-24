using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistrationFactory : IDefaultRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeCreationInfo typeCreationInfo)
        {
            return new Registration(typeCreationInfo.TypeKey, TransientLifetime.Instance, typeCreationInfo);
        }
    }
}