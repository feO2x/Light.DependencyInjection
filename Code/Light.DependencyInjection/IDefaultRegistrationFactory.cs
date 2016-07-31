namespace Light.DependencyInjection
{
    public interface IDefaultRegistrationFactory
    {
        Registration CreateDefaultRegistration(TypeInstantiationInfo typeInstantiationInfo);
    }
}