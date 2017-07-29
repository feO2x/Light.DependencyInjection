namespace Light.DependencyInjection.Services
{
    public sealed class ThreadSafeContainerScopeFactory : IContainerScopeFactory
    {
        public ContainerScope CreateScope(ContainerScope parentScope = null)
        {
            return new ThreadSafeContainerScope(parentScope);
        }
    }
}