using System.Threading;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class SingletonLifetime : Lifetime
    {
        private object _instance;

        public override object ResolveInstance(ResolveContext resolveContext)
        {
            if (_instance != null)
                return _instance;

            lock (this)
            {
                if (Volatile.Read(ref _instance) != null)
                    return _instance;

                var instance = resolveContext.CreateInstance();
                Interlocked.MemoryBarrier();
                _instance = instance;
                return _instance;
            }
        }
    }
}