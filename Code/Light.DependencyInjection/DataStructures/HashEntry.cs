namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a key-value entry in a data structure that is identified by its hash code.
    /// </summary>
    /// <typeparam name="TKey">The key of the entry.</typeparam>
    /// <typeparam name="TValue">The value </typeparam>
    public struct HashEntry<TKey, TValue>
    {
        /// <summary>
        ///     Gets the hash code of the entry.
        /// </summary>
        public readonly int HashCode;

        /// <summary>
        ///     Gets the key of the entry.
        /// </summary>
        public readonly TKey Key;

        /// <summary>
        ///     Gets the value of the entry.
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        ///     Initializes a new instance of <see cref="HashEntry{TKey,TValue}" />.
        /// </summary>
        /// <param name="hashCode">The hash code of the entry.</param>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value of the entry.</param>
        public HashEntry(int hashCode, TKey key, TValue value)
        {
            HashCode = hashCode;
            Key = key;
            Value = value;
        }

        /// <summary>
        ///     Returns the string representation of the hash entry.
        /// </summary>
        public override string ToString()
        {
            return $"[{Key}] = {Value} (hash: {HashCode})";
        }
    }
}