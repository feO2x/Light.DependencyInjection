using System;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of resolving a child value through a recursive call.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        ///     Resolves the type with the specified registration name using the resolve context.
        /// </summary>
        object Resolve(Type type, string registrationName, ResolveContext context);
    }
}