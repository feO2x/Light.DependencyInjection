using System.Threading;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerThreadLifetime : Lifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = resolveContext.CreateInstance();

            return _threadLocal.Value;
        }
    }
}