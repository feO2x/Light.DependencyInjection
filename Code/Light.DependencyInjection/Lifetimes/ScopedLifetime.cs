using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that creates instances once per container lifetime.
    /// </summary>
    public sealed class ScopedLifetime : Lifetime
    {
        /// <summary>
        ///     Gets the singleton instance of this lifetime.
        /// </summary>
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        /// <summary>
        ///     Creates a new instance if the is non for the current container scope, otherwise the existing instance will be returned.
        /// </summary>
        public override object GetInstance(CreationContext context)
        {
            return context.Container.Scope.GetOrAddScopedInstance(context.Registration.TypeKey,
                                                                  context.CreateInstance);
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