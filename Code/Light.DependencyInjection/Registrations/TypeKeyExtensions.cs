namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Provides extension methods for the <see cref="TypeKey" /> struct.
    /// </summary>
    public static class TypeKeyExtensions
    {
        /// <summary>
        ///     Gets the full registration name in the following format: "TypeKey.Type.FullName" with name "TypeKey.RegistrationName".
        ///     The last part will be ommited if registration name is null.
        /// </summary>
        public static string GetFullRegistrationName(this TypeKey typeKey)
        {
            return $"\"{typeKey.Type}\"{typeKey.GetWithRegistrationNameText()}";
        }

        /// <summary>
        ///     Gets the following text or an empty string depending on typeKey.RegistrationName being null: with name "TypeKey.RegistrationName".
        /// </summary>
        public static string GetWithRegistrationNameText(this TypeKey typeKey)
        {
            return typeKey.RegistrationName == null ? "" : $" with name \"{typeKey.RegistrationName}\"";
        }
    }
}