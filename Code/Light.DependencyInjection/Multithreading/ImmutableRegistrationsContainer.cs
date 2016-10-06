using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class ImmutableRegistrationsContainer<TRegistration>
    {
        private readonly ImmutableAvlNode<TypeKey, TRegistration>[] _buckets;
        private readonly IGrowBucketContainerStrategy<TypeKey, TRegistration> _growContainerStrategy;
        private readonly Lazy<IReadOnlyList<TypeKey>> _lazyKeys;
        private readonly Lazy<IReadOnlyList<TRegistration>> _lazyValues;
        public readonly int Count;
        public readonly bool IsEmpty;
        public readonly int NumberOfBuckets;

        private ImmutableRegistrationsContainer(IGrowBucketContainerStrategy<TypeKey, TRegistration> growContainerStrategy)
        {
            _growContainerStrategy = growContainerStrategy;
            Count = 0;
            NumberOfBuckets = 0;
            IsEmpty = true;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<TRegistration>>(CreateValues);
        }

        private ImmutableRegistrationsContainer(ImmutableAvlNode<TypeKey, TRegistration>[] buckets,
                                                int count,
                                                IGrowBucketContainerStrategy<TypeKey, TRegistration> growContainerStrategy)
        {
            _buckets = buckets;
            NumberOfBuckets = buckets.Length;
            Count = count;
            _growContainerStrategy = growContainerStrategy;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<TRegistration>>(CreateValues);
        }

        public IReadOnlyList<TypeKey> TypeKeys => _lazyKeys.Value;

        public IReadOnlyList<TRegistration> Registrations => _lazyValues.Value;

        public ImmutableAvlNode<TypeKey, TRegistration> this[int index] => _buckets[index];

        private IReadOnlyList<TypeKey> CreateKeys()
        {
            var keys = new TypeKey[Count];
            if (IsEmpty)
                return keys;

            var currentIndex = 0;
            foreach (var avlTree in _buckets)
            {
                avlTree.TraverseInOrder(node => keys[currentIndex++] = node.HashEntry.Key);
            }
            return keys;
        }

        private IReadOnlyList<TRegistration> CreateValues()
        {
            var values = new TRegistration[Count];
            if (IsEmpty)
                return values;

            var currentIndex = 0;
            foreach (var avlTree in _buckets)
            {
                avlTree.TraverseInOrder(node => values[currentIndex++] = node.HashEntry.Value);
            }
            return values;
        }

        private ImmutableAvlNode<TypeKey, TRegistration>[] CreateNewBuckets(bool tryGrow)
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

        private static ImmutableAvlNode<TypeKey, TRegistration>[] InitializeBuckets(int numberOfBuckets)
        {
            var buckets = new ImmutableAvlNode<TypeKey, TRegistration>[numberOfBuckets];
            for (var i = 0; i < numberOfBuckets; i++)
            {
                buckets[i] = ImmutableAvlNode<TypeKey, TRegistration>.Empty;
            }
            return buckets;
        }

        private void ReclassifyHashEntries(ImmutableAvlNode<TypeKey, TRegistration>[] newBuckets)
        {
            foreach (var immutableAvlTree in _buckets)
            {
                immutableAvlTree.TraverseInOrder(node => AddEntry(node.HashEntry, newBuckets));
            }
        }

        private static void AddEntry(HashEntry<TypeKey, TRegistration> hashEntry, ImmutableAvlNode<TypeKey, TRegistration>[] buckets)
        {
            var targetIndex = GetTargetBucketIndex(hashEntry.Key, buckets.Length);
            buckets[targetIndex] = buckets[targetIndex].Add(hashEntry);
        }

        private static int GetTargetBucketIndex(TypeKey typeKey, int numberOfBuckets)
        {
            return typeKey.TypeHashCode & numberOfBuckets - 1;
        }

        private static int GetTargetBucketIndex(Type type, int numberOfBuckets)
        {
            return type.GetHashCode() & numberOfBuckets - 1;
        }

        public bool TryFind(TypeKey typeKey, out TRegistration registration)
        {
            if (IsEmpty)
            {
                registration = default(TRegistration);
                return false;
            }

            var targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
            return _buckets[targetBucketIndex].TryFind(typeKey.HashCode, typeKey, out registration);
        }

        public bool GetOrAdd(TypeKey typeKey,
                             Func<TRegistration> createRegistration,
                             out TRegistration registration,
                             out ImmutableRegistrationsContainer<TRegistration> newBucketContainer)
        {
            createRegistration.MustNotBeNull(nameof(createRegistration));

            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
                if (_buckets[targetBucketIndex].TryFind(typeKey.HashCode, typeKey, out registration))
                {
                    newBucketContainer = null;
                    return false;
                }
            }
            var newBuckets = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(typeKey, newBuckets.Length);
            registration = createRegistration();
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TypeKey, TRegistration>(typeKey.HashCode, typeKey, registration));
            newBucketContainer = new ImmutableRegistrationsContainer<TRegistration>(newBuckets, Count + 1, _growContainerStrategy);
            return true;
        }

        public bool AddOrReplace(TypeKey typeKey,
                                 TRegistration registration,
                                 out ImmutableRegistrationsContainer<TRegistration> newBucketContainer)
        {
            ImmutableAvlNode<TypeKey, TRegistration>[] newBuckets;
            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
                var targetNode = _buckets[targetBucketIndex];
                if (targetNode.TryFind(typeKey.HashCode, typeKey))
                {
                    newBuckets = CreateNewBuckets(false);
                    newBuckets[targetBucketIndex] = targetNode.Replace(new HashEntry<TypeKey, TRegistration>(typeKey.HashCode, typeKey, registration));
                    newBucketContainer = new ImmutableRegistrationsContainer<TRegistration>(newBuckets, Count, _growContainerStrategy);
                    return false;
                }
            }
            newBuckets = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(typeKey, newBuckets.Length);
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TypeKey, TRegistration>(typeKey.HashCode, typeKey, registration));
            newBucketContainer = new ImmutableRegistrationsContainer<TRegistration>(newBuckets, Count + 1, _growContainerStrategy);
            return true;
        }

        public static ImmutableRegistrationsContainer<TRegistration> CreateEmpty()
        {
            return new ImmutableRegistrationsContainer<TRegistration>(new PrimeNumberLinearStrategy<TypeKey, TRegistration>());
        }

        public static ImmutableRegistrationsContainer<TRegistration> CreateEmpty(IGrowBucketContainerStrategy<TypeKey, TRegistration> growContainerStrategy)
        {
            growContainerStrategy.MustNotBeNull(nameof(growContainerStrategy));

            return new ImmutableRegistrationsContainer<TRegistration>(growContainerStrategy);
        }

        public IEnumerable<TRegistration> FindAll(Type type)
        {
            var targetBucketIndex = GetTargetBucketIndex(type, _buckets.Length);
            var targetTree = _buckets[targetBucketIndex];
            foreach (var node in targetTree.TraverseInOrder())
            {
                if (node.HashEntry.Key.Type == type)
                    yield return node.HashEntry.Value;
            }
        }
    }
}