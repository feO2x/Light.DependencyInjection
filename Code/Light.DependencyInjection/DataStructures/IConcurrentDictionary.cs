using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    public interface IConcurrentDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key] { get; set; }

        TValue AddOrUpdate(TKey key, TValue value);
        bool ContainsKey(TKey key);
        TValue GetOrAdd(TKey key, TValue value);
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
        bool TryAdd(TKey key, TValue value);
        bool TryGetValue(TKey key, out TValue value);
    }
}