using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class ImmutableBucketContainer<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private readonly ImmutableAvlNode<TKey, TValue>[] _buckets;
        private readonly IGrowBucketContainerStrategy<TKey, TValue> _growContainerStrategy;
        private readonly Lazy<IReadOnlyList<TKey>> _lazyKeys;
        private readonly Lazy<IReadOnlyList<TValue>> _lazyValues;
        public readonly int Count;
        public readonly int NumberOfBuckets;
        public readonly bool IsEmpty;

        private ImmutableBucketContainer(IGrowBucketContainerStrategy<TKey, TValue> growContainerStrategy)
        {
            _growContainerStrategy = growContainerStrategy;
            Count = 0;
            NumberOfBuckets = 0;
            IsEmpty = true;
        }

        private ImmutableBucketContainer(ImmutableAvlNode<TKey, TValue>[] buckets,
                                         IGrowBucketContainerStrategy<TKey, TValue> growContainerStrategy,
                                         int count)
        {
            _buckets = buckets;
            _growContainerStrategy = growContainerStrategy;
            NumberOfBuckets = _buckets.Length;
            Count = count;
            _lazyKeys = new Lazy<IReadOnlyList<TKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<TValue>>(CreateValues);
        }

        public ImmutableAvlNode<TKey, TValue> this[int index] => _buckets[index];

        public IReadOnlyList<TKey> Keys => _lazyKeys.Value;

        public IReadOnlyList<TValue> Values => _lazyValues.Value;

        private IReadOnlyList<TKey> CreateKeys()
        {
            var keys = new TKey[Count];
            if (IsEmpty)
                return keys;

            var currentIndex = 0;
            foreach (var avlTree in _buckets)
            {
                avlTree.TraverseInOrder(node => keys[currentIndex++] = node.HashEntry.Key);
            }
            return keys;
        }

        private IReadOnlyList<TValue> CreateValues()
        {
            var values = new TValue[Count];
            if (IsEmpty)
                return values;

            var currentIndex = 0;
            foreach (var avlTree in _buckets)
            {
                avlTree.TraverseInOrder(node => values[currentIndex++] = node.HashEntry.Value);
            }
            return values;
        }

        private ImmutableAvlNode<TKey, TValue>[] CreateNewBuckets(bool tryGrow)
        {
            var numberOfBuckets = tryGrow ? _growContainerStrategy.GetNumberOfBuckets(_buckets) : _buckets.Length;
            var newBuckets = InitializeBuckets(numberOfBuckets);
            if (_buckets == null)
                return newBuckets;

            if (numberOfBuckets == _buckets.Length)
                Array.Copy(_buckets, newBuckets, numberOfBuckets);
            else
                ReclassifyHashEntries(newBuckets);
            return newBuckets;
        }

        private static ImmutableAvlNode<TKey, TValue>[] InitializeBuckets(int numberOfBuckets)
        {
            var buckets = new ImmutableAvlNode<TKey, TValue>[numberOfBuckets];
            for (var i = 0; i < numberOfBuckets; i++)
            {
                buckets[i] = ImmutableAvlNode<TKey, TValue>.Empty;
            }
            return buckets;
        }

        private void ReclassifyHashEntries(ImmutableAvlNode<TKey, TValue>[] newBuckets)
        {
            foreach (var immutableAvlTree in _buckets)
            {
                immutableAvlTree.TraverseInOrder(node => AddEntry(node.HashEntry, newBuckets));
            }
        }

        private static void AddEntry(HashEntry<TKey, TValue> hashEntry, ImmutableAvlNode<TKey, TValue>[] buckets)
        {
            var targetIndex = GetTargetBucketIndex(hashEntry.HashCode, buckets.Length);
            buckets[targetIndex] = buckets[targetIndex].Add(hashEntry);
        }

        private static int GetTargetBucketIndex(int hashCode, int numberOfBuckets)
        {
            return hashCode & numberOfBuckets - 1;
        }

        public static ImmutableBucketContainer<TKey, TValue> CreateEmpty()
        {
            return new ImmutableBucketContainer<TKey, TValue>(new PrimeNumberLinearStrategy<TKey, TValue>());
        }

        public static ImmutableBucketContainer<TKey, TValue> CreateEmpty(IGrowBucketContainerStrategy<TKey, TValue> growContainerStrategy)
        {
            growContainerStrategy.MustNotBeNull(nameof(growContainerStrategy));

            return new ImmutableBucketContainer<TKey, TValue>(growContainerStrategy);
        }

        public bool GetOrAdd(int hashCode, TKey key, Func<TValue> createValue, out TValue value, out ImmutableBucketContainer<TKey, TValue> newBucketContainer)
        {
            createValue.MustNotBeNull(nameof(createValue));

            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(hashCode, _buckets.Length);
                if (_buckets[targetBucketIndex].TryFind(hashCode, key, out value))
                {
                    newBucketContainer = null;
                    return false;
                }
            }
            var newBuckets = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(hashCode, newBuckets.Length);
            value = createValue();
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TKey, TValue>(hashCode, key, value));
            newBucketContainer = new ImmutableBucketContainer<TKey, TValue>(newBuckets, _growContainerStrategy, Count + 1);
            return true;
        }

        public bool AddOrReplace(int hashCode, TKey key, TValue value, out ImmutableBucketContainer<TKey, TValue> newBucketContainer)
        {
            ImmutableAvlNode<TKey, TValue>[] newBuckets;
            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(hashCode, _buckets.Length);
                var targetNode = _buckets[targetBucketIndex];
                if (targetNode.TryFind(hashCode, key))
                {
                    newBuckets = CreateNewBuckets(false);
                    newBuckets[targetBucketIndex] = targetNode.Replace(new HashEntry<TKey, TValue>(hashCode, key, value));
                    newBucketContainer = new ImmutableBucketContainer<TKey, TValue>(newBuckets, _growContainerStrategy, Count);
                    return false;
                }
            }
            newBuckets = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(hashCode, newBuckets.Length);
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TKey, TValue>(hashCode, key, value));
            newBucketContainer = new ImmutableBucketContainer<TKey, TValue>(newBuckets, _growContainerStrategy, Count + 1);
            return true;
        }

        public bool TryFind(int hashCode, TKey key, out TValue value)
        {
            if (IsEmpty)
            {
                value = default(TValue);
                return false;
            }

            var targetBucketIndex = GetTargetBucketIndex(hashCode, _buckets.Length);
            return _buckets[targetBucketIndex].TryFind(hashCode, key, out value);
        }
    }
}