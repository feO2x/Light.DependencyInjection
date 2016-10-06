using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ListFactory : ICollectionFactory
    {
        private readonly MethodInfo _genericCreateDefaultCollectionMethod = typeof(ListFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateList));

        public IList<T> CreateDefaultCollection<T>()
        {
            return CreateList<T>();
        }

        public IList CreateDefaultCollection(Type itemType)
        {
            var resolvedMethod = _genericCreateDefaultCollectionMethod.MakeGenericMethod(itemType);
            return (IList) resolvedMethod.Invoke(null, null);
        }

        private static IList<T> CreateList<T>()
        {
            return new List<T>();
        }
    }
}