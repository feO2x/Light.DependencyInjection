using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the abstraction for creating a default registration for a resolved type
    ///     that has not been registered with the DI container before.
    /// </summary>
    public interface IDefaultRegistrationFactory
    {
        /// <summary>
        ///     Creates the default registration for the specified type info.
        /// </summary>
        Registration CreateDefaultRegistration(TypeCreationInfo typeCreationInfo);
    }
}