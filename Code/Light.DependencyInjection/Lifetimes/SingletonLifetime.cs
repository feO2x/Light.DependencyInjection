using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class SingletonLifetime : ILifetime
    {
        private object _instance;

        public object GetInstance(ResolveContext context)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = context.CreateInstance();
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