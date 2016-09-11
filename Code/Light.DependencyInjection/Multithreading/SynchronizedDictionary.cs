using System;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class SynchronizedDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private readonly object _bucketContainerLock = new object();
        private ImmutableBucketContainer<TKey, TValue> _bucketContainer;

        public SynchronizedDictionary()
            : this(SynchronizedDictionaryOptions<TKey, TValue>.Create()) { }

        public SynchronizedDictionary(SynchronizedDictionaryOptions<TKey, TValue> options)
        {
            _bucketContainer = ImmutableBucketContainer<TKey, TValue>.CreateEmpty(options.GrowContainerStrategy);
        }

        public SynchronizedDictionary(SynchronizedDictionary<TKey, TValue> other)
        {
            _bucketContainer = other._bucketContainer;
        }

        public IReadOnlyList<TKey> Keys
        {
            get
            {
                var bucketContainer = _bucketContainer;
                return bucketContainer.Keys;
            }
        }

        public IReadOnlyList<TValue> Values
        {
            get
            {
                var bucketContainer = _bucketContainer;
                return bucketContainer.Values;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var bucketContainer = _bucketContainer;
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

            Monitor.Enter(_bucketContainerLock);
            ImmutableBucketContainer<TKey, TValue> newBucketContainer;
            if (_bucketContainer.GetOrAdd(hashCode, key, out value, out newBucketContainer, createValue) == false)
            {
                Monitor.Exit(_bucketContainerLock);
                return false;
            }

            _bucketContainer = newBucketContainer;
            Monitor.Exit(_bucketContainerLock);
            return true;
        }


        public bool AddOrReplace(TKey key, TValue value)
        {
            var hashCode = key.GetHashCode();

            Monitor.Enter(_bucketContainerLock);
            ImmutableBucketContainer<TKey, TValue> newBucketContainer;
            var result = _bucketContainer.AddOrReplace(hashCode, key, value, out newBucketContainer);
            _bucketContainer = newBucketContainer;
            Monitor.Exit(_bucketContainerLock);
            return result;
        }
    }
}