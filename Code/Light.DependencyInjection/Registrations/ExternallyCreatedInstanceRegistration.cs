using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ExternallyCreatedInstanceRegistration : Registration
    {
        public readonly object Instance;

        public ExternallyCreatedInstanceRegistration(object instance, string registrationName = null) : base(TypeCreationInfo.FromExternalInstance(instance.GetType()), registrationName)
        {
            Instance = instance;
        }

        public override object GetInstance(DiContainer container)
        {
            return Instance;
        }
    }
}