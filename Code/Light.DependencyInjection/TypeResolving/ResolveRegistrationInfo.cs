using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ResolveRegistrationInfo : ResolveInfo
    {
        public readonly Registration Registration;

        public ResolveRegistrationInfo(TypeKey requestedTypeKey, Registration registration) : base(requestedTypeKey)
        {
            Registration = registration.MustNotBeNull(nameof(registration));
        }
    }
}