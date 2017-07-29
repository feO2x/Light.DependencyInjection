using Light.DependencyInjection.TypeResolving;

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
        ///     Always creates a new instance using the specified delegate.
        /// </summary>
        public override object ResolveInstance(ResolveContext resolveContext)
        {
            return resolveContext.CreateInstance();
        }
    }
}