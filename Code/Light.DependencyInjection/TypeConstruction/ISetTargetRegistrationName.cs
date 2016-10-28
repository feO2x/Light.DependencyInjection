namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of changing the target registration name for a resolved child value.
    /// </summary>
    public interface ISetTargetRegistrationName
    {
        /// <summary>
        ///     Sets the target registration name to the specified value.
        /// </summary>
        string TargetRegistrationName { set; }
    }
}