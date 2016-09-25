using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class TransientLifetime : ILifetime
    {
        public static readonly TransientLifetime Instance = new TransientLifetime();

        public object GetInstance(ResolveContext context)
        {
            return context.CreateInstance();
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return this;
        }
    }
}