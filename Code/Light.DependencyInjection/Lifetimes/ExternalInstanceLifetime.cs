using System;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ExternalInstanceLifetime : ILifetime
    {
        private readonly object _value;

        public ExternalInstanceLifetime(object value)
        {
            _value = value;
        }

        public object GetInstance(ResolveContext context)
        {
            return _value;
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            throw new NotSupportedException("A lifetime with an external value cannot be attached to a registration for a generic type definition.");
        }
    }
}