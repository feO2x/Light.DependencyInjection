using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Threading
{
    public sealed class ThreadSafeContainerScopeFactory : IContainerScopeFactory
    {
        public ContainerScope CreateScope(ContainerScope parentScope = null)
        {
            return new ThreadSafeContainerScope(parentScope);
        }
    }
}