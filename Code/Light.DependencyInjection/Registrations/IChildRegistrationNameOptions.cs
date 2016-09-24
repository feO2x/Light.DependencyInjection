namespace Light.DependencyInjection.Registrations
{
    public interface IChildRegistrationNameOptions<out TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForTypes<TRegistrationOptions>
    {
        TRegistrationOptions WithName(string childValueRegistrationName);
    }
}