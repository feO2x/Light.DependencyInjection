using Light.DependencyInjection.Registrations;

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
                        _instance = registration.TypeCreationInfo.CreateInstance(container);
                }
            }
            return _instance;
        }
    }
}