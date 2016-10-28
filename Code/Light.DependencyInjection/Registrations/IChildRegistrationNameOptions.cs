namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the abstraction for setting a target registration name with a fluent API.
    /// </summary>
    /// <typeparam name="TRegistrationOptions">The registration options returned by the fluent API.</typeparam>
    public interface IChildRegistrationNameOptions<out TRegistrationOptions> where TRegistrationOptions : class, IBaseRegistrationOptionsForType<TRegistrationOptions>
    {
        /// <summary>
        ///     Sets the target registration name.
        /// </summary>
        TRegistrationOptions WithName(string childValueRegistrationName);
    }
}