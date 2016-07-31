namespace Light.DependencyInjection
{
    public sealed class TransientRegistration : Registration
    {
        public TransientRegistration(TypeInstantiationInfo typeInstantiationInfo, string registrationName = null) : base(typeInstantiationInfo, registrationName) { }

        public override object GetInstance(DiContainer container)
        {
            return TypeInstantiationInfo.InstatiateObject(container);
        }
    }
}