using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public static class CollectionExtensions
    {
        public static IList<T> AddOrReplace<T>(this IList<T> list, T item)
        {
            var index = list.MustNotBeNull(nameof(list)).IndexOf(item);
            if (index == -1)
                list.Add(item);
            else
                list[index] = item;

            return list;
        }
    }
}