namespace Light.DependencyInjection.Multithreading
{
    public struct HashEntry<TKey, TValue>
    {
        public readonly int HashCode;
        public readonly TKey Key;
        public readonly TValue Value;

        public HashEntry(int hashCode, TKey key, TValue value)
        {
            HashCode = hashCode;
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"[{Key}] = {Value} (hash: {HashCode})";
        }
    }
}