using System.Threading;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerThreadLifetime : ILifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        public object GetInstance(Registration registration, DiContainer container)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = registration.TypeCreationInfo.CreateInstance(container);

            return _threadLocal.Value;
        }
    }
}