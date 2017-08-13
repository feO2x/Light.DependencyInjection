using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class PerResolveLifetime : Lifetime
    {
        public static readonly PerResolveLifetime Instance = new PerResolveLifetime();

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            return resolveContext.GetOrCreatePerResolveInstance(resolveContext.Registration.TypeKey);
        }

        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            return Instance;
        }
    }
}