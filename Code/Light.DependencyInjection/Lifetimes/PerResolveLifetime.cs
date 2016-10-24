using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerResolveLifetime : Lifetime
    {
        public static readonly PerResolveLifetime Instance = new PerResolveLifetime();

        public override object GetInstance(ResolveContext context)
        {
            return context.GetOrCreatePerResolveInstance();
        }

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            return this;
        }
    }
}