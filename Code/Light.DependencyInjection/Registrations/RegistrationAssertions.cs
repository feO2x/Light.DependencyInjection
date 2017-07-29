using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public static class RegistrationAssertions
    {
        public static Type MustBeBaseTypeOf(this Type baseClassOrInterfaceType, Type type, IEqualityComparer<Type> typeComparer = null)
        {
            if (type.IsDerivingFromOrImplementing(baseClassOrInterfaceType, typeComparer))
                return baseClassOrInterfaceType;

            throw new TypeRegistrationException($"Type \"{baseClassOrInterfaceType}\" cannot be used as an abstraction for type \"{type}\" because the latter type does not derive from or implement the former one.", type);
        }
    }
}