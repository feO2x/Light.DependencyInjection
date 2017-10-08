namespace Light.DependencyInjection.Services
{
    public struct ChildContainerOptions
    {
        public readonly bool DetachRegistrationMappingsFromParentContainer;
        public readonly bool DetachResolveDelegatesFromParentContainer;
        public readonly ContainerServices NewContainerServices;

        public ChildContainerOptions(bool detachRegistrationMappingsFromParentContainer,
                                     bool detachResolveDelegatesFromParentContainer = false,
                                     ContainerServices newContainerServices = null)
        {
            DetachRegistrationMappingsFromParentContainer = detachRegistrationMappingsFromParentContainer;
            DetachResolveDelegatesFromParentContainer = detachRegistrationMappingsFromParentContainer || detachResolveDelegatesFromParentContainer;
            NewContainerServices = newContainerServices;
        }
    }
}