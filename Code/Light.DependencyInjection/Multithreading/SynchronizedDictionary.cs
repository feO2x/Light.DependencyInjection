using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class SynchronizedDictionary<TKey, TValue>
    {
        private readonly int _bucketCapacity;
        private readonly object _bucketLockObject = new object();
        private readonly float _growthRate;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly List<TValue> _values;
        private Bucket[] _buckets;

        public SynchronizedDictionary()
            : this(SynchronizedDictionaryOptions<TKey>.Create()) { }

        public SynchronizedDictionary(SynchronizedDictionaryOptions<TKey> options)
        {
            _bucketCapacity = options.BucketCapacity;
            _buckets = InitializeBuckets(options.InitialNumberOfBuckets, _bucketCapacity);
            _values = new List<TValue>(options.InitialNumberOfBuckets);
            _keyComparer = options.KeyComparer;
            _growthRate = options.GrowthRate;
        }

        public SynchronizedDictionary(SynchronizedDictionary<TKey, TValue> other)
        {
            Monitor.Enter(other._bucketLockObject);
            var otherBuckets = other._buckets;
            _values = other._values.ToList();
            Monitor.Exit(other._bucketLockObject);

            _keyComparer = other._keyComparer;
            _bucketCapacity = other._bucketCapacity;
            _growthRate = other._growthRate;
            _buckets = InitializeBuckets(otherBuckets.Length, _bucketCapacity);

            for (var i = 0; i < otherBuckets.Length; ++i)
            {
                var otherBucket = otherBuckets[i];
                var myBucket = _buckets[i];
                for (var j = 0; j < otherBucket.Count; j++)
                {
                    var otherElement = otherBucket[j];
                    myBucket.Add(otherElement.HashCode, otherElement.Key, otherElement.Value);
                }
            }
        }

        public IReadOnlyList<TValue> Values
        {
            get
            {
                Monitor.Enter(_bucketLockObject);
                var returnValue = _values.ToList();
                Monitor.Exit(_bucketLockObject);
                return returnValue;
            }
        }

        private static Bucket[] InitializeBuckets(int numberOfBuckets, int bucketCapacity)
        {
            var buckets = new Bucket[numberOfBuckets];
            for (var i = 0; i < numberOfBuckets; ++i)
            {
                buckets[i] = new Bucket(bucketCapacity);
            }
            return buckets;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var buckets = _buckets;
            var targetBucketIndex = GetTargetBucketIndex(key.GetHashCode(), buckets.Length);
            return buckets[targetBucketIndex].TryFind(key, _keyComparer, out value);
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
            var hashCode = _keyComparer.GetHashCode(key);

            Monitor.Enter(_bucketLockObject);
            var targetBucket = _buckets[GetTargetBucketIndex(hashCode, _buckets.Length)];
            if (targetBucket.TryFind(key, _keyComparer, out value))
            {
                Monitor.Exit(_bucketLockObject);
                return false;
            }
            if (targetBucket.IsFull)
            {
                GrowDictionary();
                targetBucket = _buckets[GetTargetBucketIndex(hashCode, _buckets.Length)];
            }
            value = createValue();
            targetBucket.Add(hashCode, key, value);
            _values.Add(value);
            Monitor.Exit(_bucketLockObject);
            return true;
        }


        public void AddOrReplace(TKey key, TValue value)
        {
            var hashCode = _keyComparer.GetHashCode(key);

            Monitor.Enter(_bucketLockObject);
            var targetBucket = _buckets[GetTargetBucketIndex(hashCode, _buckets.Length)];
            for (var i = 0; i < targetBucket.Count; ++i)
            {
                if (_keyComparer.Equals(targetBucket[i].Key, key) == false)
                    continue;

                Replace(targetBucket, i, hashCode, key, value);
                Monitor.Exit(_bucketLockObject);
                return;
            }

            Add(targetBucket, hashCode, key, value);
            Monitor.Exit(_bucketLockObject);
        }

        private void Add(Bucket targetBucket, int hashCode, TKey key, TValue value)
        {
            if (targetBucket.IsFull)
            {
                GrowDictionary();
                targetBucket = _buckets[GetTargetBucketIndex(hashCode, _buckets.Length)];
            }

            targetBucket.Add(hashCode, key, value);
            _values.Add(value);
        }

        private void Replace(Bucket targetBucket, int elementIndex, int hashCode, TKey key, TValue value)
        {
            _values.Remove(targetBucket[elementIndex].Value);
            targetBucket[elementIndex] = new BucketElement(hashCode, key, value);
        }

        private void GrowDictionary()
        {
            var currentNumberOfBuckets = _buckets.Length;
            while (true)
            {
                Start:
                var newNumberOfBuckets = (int) (currentNumberOfBuckets * _growthRate);
                if (newNumberOfBuckets < 0)
                    newNumberOfBuckets = int.MaxValue;
                else
                {
                    if (newNumberOfBuckets % 2 == 0)
                        ++newNumberOfBuckets;
                    while (newNumberOfBuckets % 3 == 0 || newNumberOfBuckets % 5 == 0 || newNumberOfBuckets % 7 == 0)
                        newNumberOfBuckets += 2;
                }


                var newBuckets = InitializeBuckets(newNumberOfBuckets, _bucketCapacity);
                foreach (var bucket in _buckets)
                {
                    for (var i = 0; i < bucket.Count; i++)
                    {
                        var bucketElement = bucket[i];
                        var newBucketIndex = GetTargetBucketIndex(bucketElement.HashCode, newBuckets.Length);
                        var newTargetBucket = newBuckets[newBucketIndex];
                        if (newTargetBucket.IsFull == false)
                            newTargetBucket.Add(bucketElement.HashCode, bucketElement.Key, bucketElement.Value);
                        else
                        {
                            currentNumberOfBuckets = newNumberOfBuckets;
                            goto Start;
                        }
                    }
                }
                _buckets = newBuckets;
                return;
            }
        }

        private static int GetTargetBucketIndex(int hashCode, int numberOfBuckets)
        {
            return (hashCode & 0x7fffffff) % numberOfBuckets;
        }

        private sealed class Bucket
        {
            private readonly BucketElement[] _elements;
            private int _count;

            public Bucket(int capacity)
            {
                _elements = new BucketElement[capacity];
                _count = 0;
            }

            public bool IsFull => _count == _elements.Length;
            public int Count => _count;

            public BucketElement this[int index]
            {
                get { return _elements[index]; }
                set { _elements[index] = value; }
            }

            public void Add(int hashCode, TKey key, TValue value)
            {
                _elements[_count++] = new BucketElement(hashCode, key, value);
            }

            public bool TryFind(TKey key, IEqualityComparer<TKey> keyComparer, out TValue value)
            {
                for (var i = 0; i < _count; i++)
                {
                    if (keyComparer.Equals(key, _elements[i].Key) == false)
                        continue;

                    value = _elements[i].Value;
                    return true;
                }
                value = default(TValue);
                return false;
            }
        }

        private struct BucketElement
        {
            public readonly int HashCode;
            public readonly TKey Key;
            public readonly TValue Value;

            public BucketElement(int hashCode, TKey key, TValue value)
            {
                HashCode = hashCode;
                Key = key;
                Value = value;
            }
        }
    }
}