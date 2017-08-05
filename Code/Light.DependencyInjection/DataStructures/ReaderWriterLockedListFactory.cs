using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ReaderWriterLockedListFactory : IConcurrentListFactory
    {
        public IConcurrentList<T> Create<T>()
        {
            return new ReaderWriterLockedList<T>();
        }

        public IConcurrentList<T> Create<T>(IEnumerable<T> items)
        {
            return new ReaderWriterLockedList<T>(items);
        }
    }
}