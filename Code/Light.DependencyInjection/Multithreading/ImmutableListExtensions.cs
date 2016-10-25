using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public static class ImmutableListExtensions
    {
        public static bool TryFind<TKey, TValue>(this ImmutableList<HashEntry<TKey, TValue>> immutableList, TKey key, out TValue value) where TKey : IEquatable<TKey>
        {
            immutableList.MustNotBeNull(nameof(immutableList));

            foreach (var entry in immutableList)
            {
                if (key.Equals(entry.Key) == false)
                    continue;

                value = entry.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public static bool ContainsEntryWithKey<TKey, TValue>(this ImmutableList<HashEntry<TKey, TValue>> immutableList, TKey key) where TKey : IEquatable<TKey>
        {
            immutableList.MustNotBeNull(nameof(immutableList));

            foreach (var entry in immutableList)
            {
                if (key.Equals(entry.Key))
                    return true;
            }

            return false;
        }

        public static int IndexOfEntry<TKey, TValue>(this ImmutableList<HashEntry<TKey, TValue>> immutableList, TKey key) where TKey : IEquatable<TKey>
        {
            immutableList.MustNotBeNull(nameof(immutableList));

            for (var i = 0; i < immutableList.Count; i++)
            {
                if (key.Equals(immutableList[i].Key))
                    return i;
            }
            return -1;
        }
    }
}