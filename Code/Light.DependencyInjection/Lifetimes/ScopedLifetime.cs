using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : Lifetime
    {
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        public override object ResolveInstance(ResolveContext resolveContext)
        {
            return resolveContext.Container.ContainerScope.TryGetScopedInstance(resolveContext.Registration.TypeKey, out var instance) ? instance : resolveContext.Container.ContainerScope.GetOrAddScopedInstance(resolveContext.Registration.TypeKey, resolveContext.CreateInstance);
        }
    }
}