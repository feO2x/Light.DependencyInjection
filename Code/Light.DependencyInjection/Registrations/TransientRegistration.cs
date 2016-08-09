using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistration : Registration
    {
        public TransientRegistration(TypeInstantiationInfo typeInstantiationInfo, string registrationName = null) : base(typeInstantiationInfo, registrationName) { }

        public override object GetInstance(DiContainer container)
        {
            return TypeInstantiationInfo.InstantiateObjectAndPerformInstanceInjections(container);
        }
    }
}