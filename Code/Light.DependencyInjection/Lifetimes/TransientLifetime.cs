using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class TransientLifetime : Lifetime
    {
        public static readonly TransientLifetime Instance = new TransientLifetime();

        public override object GetInstance(ResolveContext context)
        {
            return context.CreateInstance();
        }

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            return this;
        }
    }
}