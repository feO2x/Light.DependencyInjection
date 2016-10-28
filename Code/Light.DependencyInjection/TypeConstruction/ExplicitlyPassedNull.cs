namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Indicates that the client wants to inject null via <see cref="DependencyOverrides" />.
    /// </summary>
    public sealed class ExplicitlyPassedNull
    {
        /// <summary>
        ///     Gets the singleton instance of <see cref="ExplicitlyPassedNull" />.
        /// </summary>
        public static readonly ExplicitlyPassedNull Instance = new ExplicitlyPassedNull();

        private ExplicitlyPassedNull() { }
    }
}