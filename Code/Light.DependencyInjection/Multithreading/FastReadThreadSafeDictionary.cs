using System;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class FastReadThreadSafeDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private readonly object _bucketContainerLock = new object();
        private ImmutableBucketContainer<TKey, TValue> _bucketContainer;

        public FastReadThreadSafeDictionary()
            : this(FastReadThreadSafeDictionaryOptions<TKey, TValue>.Create()) { }

        public FastReadThreadSafeDictionary(FastReadThreadSafeDictionaryOptions<TKey, TValue> options)
        {
            _bucketContainer = ImmutableBucketContainer<TKey, TValue>.CreateEmpty(options.GrowContainerStrategy);
        }

        public FastReadThreadSafeDictionary(FastReadThreadSafeDictionary<TKey, TValue> other)
        {
            _bucketContainer = Volatile.Read(ref other._bucketContainer);
        }

        public IReadOnlyList<TKey> Keys
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.Keys;
            }
        }

        public IReadOnlyList<TValue> Values
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.Values;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.TryFind(key.GetHashCode(), key, out value);
        }

        public TValue GetOrAdd(TKey key, Func<TValue> createValue)
        {
            TValue returnValue;
            GetOrAdd(key, createValue, out returnValue);
            return returnValue;
        }

        public bool GetOrAdd(TKey key, Func<TValue> createValue, out TValue value)
        {
            createValue.MustNotBeNull(nameof(createValue));
            var hashCode = key.GetHashCode();
            lock (_bucketContainerLock)
            {
                ImmutableBucketContainer<TKey, TValue> newBucketContainer;
                if (_bucketContainer.GetOrAdd(hashCode, key, createValue, out value, out newBucketContainer) == false)
                    return false;

                _bucketContainer = newBucketContainer;
                return true;
            }
        }

        public bool AddOrReplace(TKey key, TValue value)
        {
            var hashCode = key.GetHashCode();
            bool result;
            lock (_bucketContainerLock)
            {
                ImmutableBucketContainer<TKey, TValue> newBucketContainer;
                result = _bucketContainer.AddOrReplace(hashCode, key, value, out newBucketContainer);
                _bucketContainer = newBucketContainer;
            }
            return result;
        }
    }
}