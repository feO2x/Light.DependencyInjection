using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class SingletonLifetime : ILifetime
    {
        private object _instance;

        public object GetInstance(Registration registration, DiContainer container)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = registration.TypeCreationInfo.CreateInstance(container, registration.IsTrackingDisposables);
                }
            }
            return _instance;
        }

        public object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = registration.TypeCreationInfo.CreateInstance(container, parameterOverrides, registration.IsTrackingDisposables);
                    else
                        throw new ResolveTypeException($"The type {registration.TypeKey.GetFullRegistrationName()} is registered as a singleton and already instantiated. Thus CreateInstance cannot be called successfully.", registration.TargetType);
                }
            }
            return _instance;
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return new SingletonLifetime();
        }
    }
}