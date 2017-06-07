using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    public interface IConcurrentListFactory
    {
        IList<T> Create<T>();
    }
}