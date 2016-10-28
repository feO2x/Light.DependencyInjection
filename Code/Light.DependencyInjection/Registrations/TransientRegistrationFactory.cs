using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents an <see cref="IDefaultRegistrationFactory" /> that creates registrations with a transient lifetime.
    /// </summary>
    public sealed class TransientRegistrationFactory : IDefaultRegistrationFactory
    {
        /// <summary>
        ///     Creates a new registration with a transient lifetime for the specified <paramref name="typeCreationInfo" />.
        /// </summary>
        public Registration CreateDefaultRegistration(TypeCreationInfo typeCreationInfo)
        {
            return new Registration(TransientLifetime.Instance, typeCreationInfo);
        }
    }
}