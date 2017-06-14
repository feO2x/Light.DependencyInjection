using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    public interface IConcurrentList<T> : IList<T>
    {
        T GetOrAdd(T item);
        void AddOrUpdate(T item);
    }
}