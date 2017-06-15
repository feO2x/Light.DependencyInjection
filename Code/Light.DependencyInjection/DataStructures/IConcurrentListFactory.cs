namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a factory that creates instances of <see cref="IConcurrentList{T}" />.
    /// </summary>
    public interface IConcurrentListFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IConcurrentList{T}" />.
        /// </summary>
        /// <typeparam name="T">The item type of the list.</typeparam>
        IConcurrentList<T> Create<T>();
    }
}