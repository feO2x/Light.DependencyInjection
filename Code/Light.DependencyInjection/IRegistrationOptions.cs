namespace Light.DependencyInjection
{
    public interface IRegistrationOptions
    {
        IRegistrationOptions WithRegistrationName(string registrationName);
        IRegistrationOptions UseDefaultConstructor();
    }
}