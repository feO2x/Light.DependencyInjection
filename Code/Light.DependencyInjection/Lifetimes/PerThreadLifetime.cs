using System.Threading;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerThreadLifetime : ILifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        public object GetInstance(ResolveContext context)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = context.CreateInstance();

            return _threadLocal.Value;
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return new PerThreadLifetime();
        }
    }
}