using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents the polymorphic abstraction of a lifetime that decides when a new instance of a concrete type is created.
    /// </summary>
    public abstract class Lifetime
    {
        private readonly string _toStringText;

        /// <summary>
        ///     Gets the value indicating whether this lifetime creates instances at some point in time.
        /// </summary>
        public readonly bool IsCreatingNewInstances;

        /// <summary>
        ///     Initializes a new instance of <see cref="Lifetime" />.
        /// </summary>
        /// <param name="isCreatingNewInstances">
        ///     The value indicating whether this lifetime creates new instances at some point when <see cref="ResolveInstance" /> is called.
        ///     This value should only be set to false when the lifetime never calls <see cref="ResolveContext.CreateNewInstance" /> within <see cref="ResolveInstance" />
        ///     and thusly, the container does not need to construct a <see cref="TypeConstructionInfo" /> for the target type.
        /// </param>
        /// <param name="toStringText">
        ///     The text that is returned when <see cref="ToString" /> is called.
        ///     If null is specified, <see cref="ToString" /> will return the name of the lifetime's type (not the full name).
        /// </param>
        protected Lifetime(bool isCreatingNewInstances = true, string toStringText = null)
        {
            IsCreatingNewInstances = isCreatingNewInstances;
            _toStringText = toStringText ?? GetType().Name;
        }

        /// <summary>
        ///     Gets the requested instance.
        /// </summary>
        public abstract object ResolveInstance(ResolveContext resolveContext);

        /// <summary>
        ///     Gets the text representation of this lifetime.
        /// </summary>
        public override string ToString()
        {
            return _toStringText;
        }
    }
}