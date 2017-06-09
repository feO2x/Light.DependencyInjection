using System;
using System.Threading;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class SingletonLifetime : Lifetime
    {
        private object _instance;

        public override object ResolveInstance(Func<object> createInstance)
        {
            if (_instance != null)
                return _instance;

            lock (this)
            {
                if (Volatile.Read(ref _instance) != null)
                    return _instance;

                var instance = createInstance();
                Interlocked.MemoryBarrier();
                _instance = instance;
                return _instance;
            }
        }
    }
}