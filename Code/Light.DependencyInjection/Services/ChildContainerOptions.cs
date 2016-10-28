namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents the options that configure how a child container is created.
    /// </summary>
    public struct ChildContainerOptions
    {
        /// <summary>
        ///     Gets the value indicating whether an empty scope should be created that is not linked to the scope of the parent container.
        /// </summary>
        public readonly bool CreateEmptyScope;

        /// <summary>
        ///     Gets the value indicating whether the mappings dictionary should be copied instead of passed by reference.
        /// </summary>
        public readonly bool CreateCopyOfMappings;

        /// <summary>
        ///     Gets the value indicating whether a shallow copy of the container services should be passed to the child container instead of the same reference.
        /// </summary>
        public readonly bool CloneContainerServices;

        /// <summary>
        ///     Creates a new instance of <see cref="ChildContainerOptions" />.
        /// </summary>
        /// <param name="createEmptyScope">The value indicating whether an empty scope should be created that is not linked to the scope of the parent container.</param>
        /// <param name="createCopyOfMappings">The value indicating whether the mappings dictionary should be copied instead of passed by reference.</param>
        /// <param name="cloneContainerServices">The value indicating whether a shallow copy of the container services should be passed to the child container instead of the same reference.</param>
        public ChildContainerOptions(bool createEmptyScope = false,
                                     bool createCopyOfMappings = false,
                                     bool cloneContainerServices = false)
        {
            CreateEmptyScope = createEmptyScope;
            CreateCopyOfMappings = createCopyOfMappings;
            CloneContainerServices = cloneContainerServices;
        }

        /// <summary>
        ///     Gets the default child container options.
        /// </summary>
        public static ChildContainerOptions Default => new ChildContainerOptions(false);
    }
}