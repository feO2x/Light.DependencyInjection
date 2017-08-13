using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class HierarchicalScopedLifetime : Lifetime
    {
        public static readonly HierarchicalScopedLifetime Instance = new HierarchicalScopedLifetime();

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            return resolveContext.Container.Scope.TryGetScopedInstance(resolveContext.Registration.TypeKey, out var instance, false) ? instance : resolveContext.Container.Scope.GetOrAddScopedInstance(resolveContext.Registration.TypeKey, resolveContext.CreateInstance, false);
        }

        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            return Instance;
        }
    }
}