namespace Light.DependencyInjection.Services
{
    public interface IContainerScopeFactory
    {
        ContainerScope CreateScope(ContainerScope parentScope = null);
    }
}