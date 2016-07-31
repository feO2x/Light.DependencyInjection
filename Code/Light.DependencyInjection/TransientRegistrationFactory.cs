namespace Light.DependencyInjection
{
    public sealed class TransientRegistrationFactory : IDefaultRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeInstantiationInfo typeInstantiationInfo)
        {
            return new TransientRegistration(typeInstantiationInfo);
        }
    }
}