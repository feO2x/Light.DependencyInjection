using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class HierarchicalScopedLifetime : Lifetime
    {
        public static readonly HierarchicalScopedLifetime Instance = new HierarchicalScopedLifetime();

        public override object ResolveInstance(ResolveContext resolveContext)
        {
            return resolveContext.Container.ContainerScope.TryGetScopedInstance(resolveContext.Registration.TypeKey, out var instance, false) ? instance : resolveContext.Container.ContainerScope.GetOrAddScopedInstance(resolveContext.Registration.TypeKey, resolveContext.CreateInstance, false);
        }
    }
}