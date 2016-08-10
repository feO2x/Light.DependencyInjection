namespace Light.DependencyInjection.Registrations
{
    public interface IChildRegistrationNameOptions<T>
    {
        IRegistrationOptions<T> WithName(string childValueRegistrationName);
    }
}