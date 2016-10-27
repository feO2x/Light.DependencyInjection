using System;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that returns an externally created instance.
    /// </summary>
    public sealed class ExternalInstanceLifetime : Lifetime
    {
        private readonly object _value;

        /// <summary>
        ///     Initializes a new instance of <see cref="ExternalInstanceLifetime" />.
        /// </summary>
        /// <param name="value">The instance that this lifetime returns.</param>
        public ExternalInstanceLifetime(object value)
        {
            _value = value;
        }

        /// <summary>
        ///     Gets the value indicating that this lifetime does not require a <see cref="TypeCreationInfo" />.
        /// </summary>
        public override bool RequiresTypeCreationInfo => false;

        /// <summary>
        ///     Return the external instance.
        /// </summary>
        public override object GetInstance(ResolveContext context)
        {
            return _value;
        }

        /// <summary>
        ///     Throws a <see cref="NotSupportedException" /> because external instances cannot describe a generic type definition.
        /// </summary>
        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            throw new NotSupportedException("A lifetime with an external value cannot be attached to a registration for a generic type definition.");
        }
    }
}