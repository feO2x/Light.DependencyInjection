namespace Light.DependencyInjection.Registrations
{
    public interface IChildRegistrationNameOptions<out TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForType<TRegistrationOptions>
    {
        TRegistrationOptions WithName(string childValueRegistrationName);
    }
}