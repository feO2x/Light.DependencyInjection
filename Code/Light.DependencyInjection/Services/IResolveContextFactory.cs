using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Services
{
    public interface IResolveContextFactory
    {
        ResolveContext Create(DiContainer container, DependencyOverrides dependencyOverrides);
    }
}