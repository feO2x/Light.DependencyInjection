using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Services
{
    public sealed class TransientResolveContextFactory : IResolveContextFactory
    {
        public ResolveContext Create(DiContainer container, DependencyOverrides dependencyOverrides = null)
        {
            return new ResolveContext(container, dependencyOverrides);
        }
    }
}