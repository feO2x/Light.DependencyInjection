using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    // TODO: allow settings for ReaderWriterLockedList<T>
    public sealed class ReaderWriterLockedListFactory : IConcurrentListFactory
    {
        public IList<T> Create<T>()
        {
            return new ReaderWriterLockedList<T>();
        }
    }
}