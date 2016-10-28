using Light.DependencyInjection.Services;

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
        ///     Returns a new instance.
        /// </summary>
        public override object GetInstance(CreationContext context)
        {
            return context.CreateInstance();
        }

        /// <summary>
        ///     Return reference to itself.
        /// </summary>
        public override Lifetime BindToClosedGenericType()
        {
            return this;
        }
    }
}