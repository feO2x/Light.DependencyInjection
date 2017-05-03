using System;
using System.Collections.Generic;
using System.Text;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration
    {
        public readonly TypeKey TypeKey;

        public Registration(TypeKey typeKey)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));

            TypeKey = typeKey;
        }
    }
}
