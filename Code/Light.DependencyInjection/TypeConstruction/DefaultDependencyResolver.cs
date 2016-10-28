using System;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="IDependencyResolver" /> that resolves a single value using the <see cref="ResolveContext" />.
    /// </summary>
    public sealed class DefaultDependencyResolver : IDependencyResolver
    {
        /// <summary>
        ///     Gets the singleton instance of this resolver.
        /// </summary>
        public static readonly DefaultDependencyResolver Instance = new DefaultDependencyResolver();

        /// <inheritdoc />
        public object Resolve(Type type, string registrationName, ResolveContext context)
        {
            return context.ResolveChildValue(new TypeKey(type, registrationName));
        }
    }
}