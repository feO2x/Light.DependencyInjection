namespace Light.DependencyInjection.Services
{
    public struct ChildContainerOptions
    {
        public readonly bool CreateEmptyScope;
        public readonly bool CreateCopyOfMappings;
        public readonly bool CloneContainerServices;

        public ChildContainerOptions(bool createEmptyScope = false,
                                     bool createCopyOfMappings = false,
                                     bool cloneContainerServices = false)
        {
            CreateEmptyScope = createEmptyScope;
            CreateCopyOfMappings = createCopyOfMappings;
            CloneContainerServices = cloneContainerServices;
        }

        public static ChildContainerOptions Default => new ChildContainerOptions(false);
    }
}