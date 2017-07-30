namespace Light.DependencyInjection.Services
{
    public sealed class DefaultDiContainerSetup : IContainerSetup
    {
        public void Setup(DiContainer container)
        {
            container.AddDefaultGuidRegistration();
        }
    }
}