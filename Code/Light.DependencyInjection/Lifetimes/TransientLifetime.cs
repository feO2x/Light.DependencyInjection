namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that always creates new instances.
    /// </summary>
    public sealed class TransientLifetime : Lifetime
    {
        /// <summary>
        ///     Gets the singleton instance of this lifetime.
        /// </summary>
        public static readonly TransientLifetime Instance = new TransientLifetime();

        /// <summary>
        ///     Requests the <paramref name="resolveContext" /> to create a new instance of the target type.
        /// </summary>
        public override object ResolveInstance(ResolveContext resolveContext)
        {
            return resolveContext.CreateNewInstance();
        }
    }
}