namespace Light.DependencyInjection.DataStructures
{
    // TODO: allow settings for ReaderWriterLockedList<T>
    public sealed class ReaderWriterLockedListFactory : IConcurrentListFactory
    {
        public IConcurrentList<T> Create<T>()
        {
            return new ReaderWriterLockedList<T>();
        }
    }
}