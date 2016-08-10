using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistration : Registration
    {
        public TransientRegistration(TypeCreationInfo typeCreationInfo, string registrationName = null) : base(typeCreationInfo, registrationName) { }

        public override object GetInstance(DiContainer container)
        {
            return TypeCreationInfo.CreateInstance(container);
        }
    }
}