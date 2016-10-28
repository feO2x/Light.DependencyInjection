using Light.DependencyInjection.Multithreading;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents an <see cref="IContainerScopeFactory" /> that creates <see cref="ThreadSafeContainerScope" /> instances.
    /// </summary>
    public sealed class ThreadSafeContainerScopeFactory : IContainerScopeFactory
    {
        /// <inheritdoc />
        public ContainerScope CreateScope(ContainerScope parentScope = null)
        {
            return new ThreadSafeContainerScope(parentScope);
        }
    }
}