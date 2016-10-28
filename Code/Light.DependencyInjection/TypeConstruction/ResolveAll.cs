using System;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="IDependencyResolver" /> that resolves all instances of the given type.
    /// </summary>
    /// <typeparam name="T">The type whose instances should be all resolved.</typeparam>
    public sealed class ResolveAll<T> : IDependencyResolver
    {
        /// <inheritdoc />
        public object Resolve(Type type, string registrationName, ResolveContext context)
        {
            return context.Container.ResolveAll<T>();
        }
    }

    /// <summary>
    ///     Provides a factory method to create a <see cref="ResolveAll{T}" /> instance from a non-generic context.
    /// </summary>
    public static class ResolveAll
    {
        /// <summary>
        ///     Creates a new <see cref="ResolveAll{T}" /> instance with T bound to the specified type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestedType" /> is null.</exception>
        public static IDependencyResolver Create(Type requestedType)
        {
            var constructor = typeof(ResolveAll<>).MakeGenericType(requestedType).GetDefaultConstructor();
            return (IDependencyResolver) constructor.Invoke(null);
        }
    }
}