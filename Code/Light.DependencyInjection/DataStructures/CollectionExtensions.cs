using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public static class CollectionExtensions
    {
        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
        {
            // ReSharper disable PossibleMultipleEnumeration
            enumerable.MustNotBeNull(nameof(enumerable));

            return enumerable as IReadOnlyList<T> ?? enumerable.ToList();
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}