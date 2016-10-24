using System;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ExternalInstanceLifetime : Lifetime
    {
        private readonly object _value;

        public ExternalInstanceLifetime(object value)
        {
            _value = value;
        }

        public override bool RequiresTypeCreationInfo => false;

        public override object GetInstance(ResolveContext context)
        {
            return _value;
        }

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            throw new NotSupportedException("A lifetime with an external value cannot be attached to a registration for a generic type definition.");
        }
    }
}