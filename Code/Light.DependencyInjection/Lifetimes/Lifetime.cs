using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents the polymorphic abstraction for lifetimes that describe when an instance is created.
    /// </summary>
    public abstract class Lifetime
    {
        private readonly string _toStringText;

        /// <summary>
        ///     Creates a new instance of <see cref="Lifetime" />.
        /// </summary>
        protected Lifetime()
        {
            _toStringText = GetType().Name;
        }

        /// <summary>
        ///     Gets the value indicating whether this lifetime requires a <see cref="TypeCreationInfo" />. The default implementation returns true.
        /// </summary>
        public virtual bool RequiresTypeCreationInfo => true;

        /// <summary>
        ///     Gets the requested instance.
        /// </summary>
        /// <param name="context">The context that can be used to resolve the instance if necessary.</param>
        public abstract object GetInstance(ResolveContext context);

        /// <summary>
        ///     Returns an instance that is used when a registration for a generic type defintion is bound to a closed generic type.
        ///     If a Lifetime implementation holds state, then a new lifetime should be returned, otherwise a singleton should be returned
        ///     to decrease the number of instances in memory.
        /// </summary>
        public abstract Lifetime ProvideInstanceForResolvedGenericTypeDefinition();

        /// <summary>
        ///     Gets the string representation of this lifetime.
        /// </summary>
        public override string ToString()
        {
            return _toStringText;
        }
    }
}