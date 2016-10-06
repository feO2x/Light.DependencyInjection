using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface ICollectionFactory
    {
        IList<T> CreateDefaultCollection<T>();
        IList CreateDefaultCollection(Type itemType);
    }
}