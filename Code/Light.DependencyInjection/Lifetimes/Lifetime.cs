using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents the polymorphic abstraction of a lifetime that decides when a new instance of a concrete type is created.
    /// </summary>
    public abstract class Lifetime
    {
        private readonly string _toStringText;

        /// <summary>
        ///     Gets the value indicating whether this lifetime can be resolved during compilation of the resolve method.
        /// </summary>
        public readonly bool CanBeResolvedDuringCompilation;

        /// <summary>
        ///     Gets the value indicating whether this lifetime creates instances at some point in time.
        /// </summary>
        public readonly bool IsCreatingNewInstances;

        /// <summary>
        ///     Initializes a new instance of <see cref="Lifetime" />.
        /// </summary>
        /// <param name="isCreatingNewInstances">
        ///     The value indicating whether this lifetime creates new instances at some point when <see cref="ResolveInstance" /> is called.
        ///     This value should only be set to false when <see cref="ResolveInstance" /> never calls <see cref="IResolveContext.CreateInstance" />
        ///     and thusly, the container does not need to construct a <see cref="TypeConstructionInfo" /> for the target type.
        /// </param>
        /// <param name="canBeResolvedDuringCompilation">
        ///     The value indicating whether the lifetime can be resolved during compilation of the resolve method. This results in a resolve method
        ///     that executes faster because the corresponding dependency is directly referenced by the resolve method, instead of calling <see cref="ResolveInstance" />
        ///     dynamically. Only set this value to true when you return the same instance on a <see cref="ResolveInstance" /> call.
        /// </param>
        /// <param name="toStringText">
        ///     The text that is returned when <see cref="ToString" /> is called.
        ///     If null is specified, <see cref="ToString" /> will return the name of the lifetime's type (not the fully qualified name).
        /// </param>
        protected Lifetime(bool isCreatingNewInstances = true, bool canBeResolvedDuringCompilation = false, string toStringText = null)
        {
            IsCreatingNewInstances = isCreatingNewInstances;
            CanBeResolvedDuringCompilation = canBeResolvedDuringCompilation;
            _toStringText = toStringText;
        }

        /// <summary>
        ///     Gets the requested instance.
        /// </summary>
        public abstract object ResolveInstance(IResolveContext resolveContext);

        /// <summary>
        ///     Gets the lifetime instance that shall be used when a generic type definition is resolved as a closed constructed generic type.
        /// </summary>
        public abstract Lifetime GetLifetimeInstanceForConstructedGenericType();

        /// <summary>
        ///     Gets the text representation of this lifetime.
        /// </summary>
        public override string ToString()
        {
            return _toStringText ?? GetType().Name;
        }
    }
}