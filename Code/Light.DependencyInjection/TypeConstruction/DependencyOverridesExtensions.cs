namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Provides an extension method to escape null values explicitely.
    /// </summary>
    public static class DependencyOverridesExtensions
    {
        /// <summary>
        ///     Returns the given value or <see cref="ExplicitlyPassedNull.Instance" /> when <paramref name="value" /> is null.
        ///     This is used to indicate that a dependency override wants to inject null into the target parameter or member.
        /// </summary>
        public static object EscapeNullIfNecessary(this object value)
        {
            return value ?? ExplicitlyPassedNull.Instance;
        }
    }
}