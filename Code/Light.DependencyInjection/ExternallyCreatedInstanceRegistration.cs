namespace Light.DependencyInjection
{
    public sealed class ExternallyCreatedInstanceRegistration : Registration
    {
        public readonly object Instance;

        public ExternallyCreatedInstanceRegistration(object instance, string registrationName = null) : base(TypeInstantiationInfo.FromExternalInstance(instance), registrationName)
        {
            Instance = instance;
        }

        public override object GetInstance(DiContainer container)
        {
            return Instance;
        }
    }
}