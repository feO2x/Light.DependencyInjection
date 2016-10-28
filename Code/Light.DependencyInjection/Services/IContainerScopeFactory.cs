namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents the abstraction for creating a <see cref="ContainerScope" />.
    /// </summary>
    public interface IContainerScopeFactory
    {
        /// <summary>
        ///     Creates a new container scope, with a possible parent scope attached to it.
        /// </summary>
        ContainerScope CreateScope(ContainerScope parentScope = null);
    }
}