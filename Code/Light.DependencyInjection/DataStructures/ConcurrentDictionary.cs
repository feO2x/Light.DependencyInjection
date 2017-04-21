namespace Light.DependencyInjection.DataStructures
{
    public sealed class ConcurrentDictionary<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    {
        public TValue AddOrUpdate(TKey key, TValue value)
        {
            return AddOrUpdate(key, value, (k, v) => value);
        }
    }
}