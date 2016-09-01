namespace Light.DependencyInjection.Registrations
{
    public static class TypeKeyExtensions
    {
        public static string GetCompleteRegistrationName(this TypeKey typeKey)
        {
            return $"\"{typeKey.Type}\"{typeKey.GetWithRegistrationNameText()}";
        }

        public static string GetWithRegistrationNameText(this TypeKey typeKey)
        {
            return typeKey.RegistrationName == null ? "" : $" with registration name \"{typeKey.RegistrationName}\"";
        }
    }
}