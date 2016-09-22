using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ExternalValueLifetime : ILifetime
    {
        private readonly object _value;

        public ExternalValueLifetime(object value)
        {
            _value = value;
        }

        public object GetInstance(Registration registration, DiContainer container)
        {
            return _value;
        }
    }
}