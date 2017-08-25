namespace Light.DependencyInjection.Registrations
{
    public interface IDependencyOptions
    {
        IDependencyOptions WithTargetRegistrationName(string registrationName);
        IDependencyOptions SetResolveAllTo(bool? resolveAll);
    }
}