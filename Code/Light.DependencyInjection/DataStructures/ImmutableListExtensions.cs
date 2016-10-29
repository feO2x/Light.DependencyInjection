using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Provides extension methods for the <see cref="ImmutableList{T}" /> class when the item type is <see cref="HashEntry{TKey,TValue}" />.
    /// </summary>
    public static class ImmutableListExtensions
    {
        /// <summary>
        ///     Tries to retrieve the value with the given key from the immutable list.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="immutableList">The immutable list that is searched.</param>
        /// <param name="key">The key that identifies the value.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>True if the value could be retrieved, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="immutableList" /> is null.</exception>
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

        /// <summary>
        ///     Checks if the immutable list contains a hash entry with the specified key.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="immutableList">The immutable list that is searched.</param>
        /// <param name="key">The key that identifies the value.</param>
        /// <returns>True if the immutable list contains an entry with the specified key, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="immutableList" /> is null.</exception>
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

        /// <summary>
        ///     Returns the index of the entry with the specified key, or -1 if no such entry exists in the immutable list.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="immutableList">The immutable list that is searched.</param>
        /// <param name="key">The key that identifies the value.</param>
        /// <returns>The index of the entry with the specified key, or -1 if no such entry exists in the immutable list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="immutableList" /> is null.</exception>
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