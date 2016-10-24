using System.Threading;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerThreadLifetime : Lifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        public override object GetInstance(ResolveContext context)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = context.CreateInstance();

            return _threadLocal.Value;
        }

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            return new PerThreadLifetime();
        }
    }
}