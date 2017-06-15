using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public static class ListExtensions
    {
        public static bool TryFindRegistration(this IList<Registration> registrations, TypeKey typeKey, out Registration registration)
        {
            registrations.MustNotBeNull(nameof(registrations));
            typeKey.MustNotBeEmpty(nameof(typeKey));

            for (var i = 0; i < registrations.Count; i++)
            {
                registration = registrations[i];
                if (registration.RegistrationName == typeKey.RegistrationName)
                    return true;
            }

            registration = null;
            return false;
        }
    }
}
