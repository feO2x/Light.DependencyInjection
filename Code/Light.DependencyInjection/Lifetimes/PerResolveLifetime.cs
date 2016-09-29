using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerResolveLifetime : ILifetime
    {
        public static readonly PerResolveLifetime Instance = new PerResolveLifetime();

        public object GetInstance(ResolveContext context)
        {
            return context.GetPerResolveInstance();
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return this;
        }
    }
}