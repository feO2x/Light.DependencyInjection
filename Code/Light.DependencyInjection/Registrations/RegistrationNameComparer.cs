using System.Collections.Generic;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationNameComparer : IEqualityComparer<Registration>
    {
        public static readonly RegistrationNameComparer Instance = new RegistrationNameComparer();

        public bool Equals(Registration x, Registration y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.RegistrationName == y.RegistrationName;
        }

        public int GetHashCode(Registration registration)
        {
            return registration?.RegistrationName.GetHashCode() ?? 0;
        }
    }
}