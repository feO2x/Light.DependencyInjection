using System.Reflection;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the abstraction for automatically selecting a constructor from a concrete target type
    ///     when the client did not specify an instantiation info explicitely.
    /// </summary>
    public interface IConstructorSelector
    {
        /// <summary>
        ///     Selects a constructor from the specified type info.
        /// </summary>
        /// <exception cref="TypeRegistrationException">Thrown when a constructor cannot be selected successfully.</exception>
        ConstructorInfo SelectTargetConstructor(TypeInfo typeInfo);
    }
}