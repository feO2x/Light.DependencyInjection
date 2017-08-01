using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class DefaultAutomaticRegistrationFactory : IAutomaticRegistrationFactory
    {
        public Registration CreateDefaultRegistration(TypeKey typeKey, DiContainer container)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));
            container.MustNotBeNull(nameof(container));

            var registrationOptions = container.Services.CreateRegistrationOptions(typeKey.Type);
            if (typeKey.RegistrationName.IsNullOrWhiteSpace() == false)
                registrationOptions.UseRegistrationName(typeKey.RegistrationName);

            return registrationOptions.CreateRegistration();
        }
    }
}