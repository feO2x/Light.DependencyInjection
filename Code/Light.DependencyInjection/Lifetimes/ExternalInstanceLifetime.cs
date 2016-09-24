using System;
using Light.DependencyInjection.Registrations;
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

        public object GetInstance(Registration registration, DiContainer container)
        {
            return _value;
        }

        public object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides)
        {
            throw new NotSupportedException("A registration with an external value cannot be reinstantiated with overridden parameters.");
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            throw new NotSupportedException("A registration with an external value cannot be a registration for a generic type definition.");
        }
    }
}