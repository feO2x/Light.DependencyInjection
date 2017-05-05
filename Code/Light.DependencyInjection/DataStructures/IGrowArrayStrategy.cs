using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    public interface IGrowArrayStrategy<T>
    {
        T[] CreateInitialArray(IEnumerable<T> existingItems);
        T[] CreateLargerArrayFrom(T[] array);
    }
}