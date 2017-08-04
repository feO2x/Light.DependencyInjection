using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Services
{
    public sealed class TransientResolveContextFactory : IResolveContextFactory
    {
        public ResolveContext Create(DiContainer container)
        {
            return new ResolveContext(container);
        }
    }
}