namespace Light.DependencyInjection.Services
{
    public sealed class DefaultContainerScopeFactory : IContainerScopeFactory
    {
        public ContainerScope CreateScope(ContainerScope parentScope = null)
        {
            return new ContainerScope(parentScope);
        }
    }
}