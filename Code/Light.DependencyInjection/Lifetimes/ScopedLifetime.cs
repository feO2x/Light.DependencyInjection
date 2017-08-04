using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : Lifetime
    {
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            return resolveContext.Container.Scope.TryGetScopedInstance(resolveContext.Registration.TypeKey, out var instance) ? instance : resolveContext.Container.Scope.GetOrAddScopedInstance(resolveContext.Registration.TypeKey, resolveContext.CreateInstance);
        }
    }
}