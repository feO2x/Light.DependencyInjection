using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class SingletonLifetime : Lifetime
    {
        private object _instance;

        public override object GetInstance(ResolveContext context)
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

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            return new SingletonLifetime();
        }
    }
}