using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Services
{
    public interface IResolveContextFactory
    {
        ResolveContext Create(DependencyInjectionContainer container, DependencyOverrides dependencyOverrides = null);
    }
}