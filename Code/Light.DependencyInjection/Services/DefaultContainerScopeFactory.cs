namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents an <see cref="IContainerScopeFactory" /> that returns <see cref="ContainerScope" /> instances.
    /// </summary>
    public sealed class DefaultContainerScopeFactory : IContainerScopeFactory
    {
        /// <inheritdoc />
        public ContainerScope CreateScope(ContainerScope parentScope = null)
        {
            return new ContainerScope(parentScope);
        }
    }
}