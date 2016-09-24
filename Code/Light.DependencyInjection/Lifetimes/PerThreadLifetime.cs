using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerThreadLifetime : ILifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        public object GetInstance(Registration registration, DiContainer container)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = registration.TypeCreationInfo.CreateInstance(container, registration.IsTrackingDisposables);

            return _threadLocal.Value;
        }

        public object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides)
        {
            if (_threadLocal.IsValueCreated)
                throw new ResolveTypeException($"The type {registration.TypeKey.GetFullRegistrationName()} is registered with a Per-Thread-Lifetime and already instantiated. Thus CreateInstance cannot be called successfully.", registration.TargetType);

            _threadLocal.Value = registration.TypeCreationInfo.CreateInstance(container, parameterOverrides, registration.IsTrackingDisposables);
            return _threadLocal.Value;
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return new PerThreadLifetime();
        }
    }
}