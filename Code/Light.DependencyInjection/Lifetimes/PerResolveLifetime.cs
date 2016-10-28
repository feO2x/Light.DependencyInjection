using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that creates instances once per resolve.
    /// </summary>
    public sealed class PerResolveLifetime : Lifetime
    {
        /// <summary>
        ///     Gets the singleton instance of this lifetime.
        /// </summary>
        public static readonly PerResolveLifetime Instance = new PerResolveLifetime();

        /// <summary>
        ///     Creates a new instance if it was not created before during a Resolve call, otherwise returns the existing instance.
        /// </summary>
        public override object GetInstance(CreationContext context)
        {
            return context.GetOrCreatePerResolveInstance();
        }

        /// <summary>
        ///     Returns reference to itself.
        /// </summary>
        public override Lifetime BindToClosedGenericType()
        {
            return this;
        }
    }
}