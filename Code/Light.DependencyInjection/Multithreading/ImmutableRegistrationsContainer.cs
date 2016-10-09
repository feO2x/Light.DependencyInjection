using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class ImmutableRegistrationsContainer<TRegistration>
    {
        private readonly ImmutableAvlNode<TRegistration>[] _buckets;
        private readonly IGrowBucketContainerStrategy<TRegistration> _growContainerStrategy;
        private readonly Lazy<IReadOnlyList<TypeKey>> _lazyKeys;
        private readonly Lazy<IReadOnlyList<TRegistration>> _lazyValues;
        public readonly int Count;
        public readonly bool IsEmpty;
        public readonly int NumberOfBuckets;

        private ImmutableRegistrationsContainer(IGrowBucketContainerStrategy<TRegistration> growContainerStrategy)
        {
            _growContainerStrategy = growContainerStrategy;
            Count = 0;
            NumberOfBuckets = 0;
            IsEmpty = true;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<TRegistration>>(CreateValues);
        }

        private ImmutableRegistrationsContainer(ImmutableAvlNode<TRegistration>[] buckets,
                                                int count,
                                                IGrowBucketContainerStrategy<TRegistration> growContainerStrategy)
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

        public ImmutableAvlNode<TRegistration> this[int index] => _buckets[index];

        private IReadOnlyList<TypeKey> CreateKeys()
        {
            var keys = new TypeKey[Count];
            if (IsEmpty)
                return keys;

            var currentIndex = 0;
            foreach (var avlTree in _buckets)
            {
                foreach (var node in avlTree)
                {
                    keys[currentIndex++] = node.HashEntry.Key;
                }
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
                foreach (var node in avlTree)
                {
                    values[currentIndex++] = node.HashEntry.Value;
                }
            }
            return values;
        }

        private ImmutableAvlNode<TRegistration>[] CreateNewBuckets(bool tryGrow)
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

        private static ImmutableAvlNode<TRegistration>[] InitializeBuckets(int numberOfBuckets)
        {
            var buckets = new ImmutableAvlNode<TRegistration>[numberOfBuckets];
            for (var i = 0; i < numberOfBuckets; i++)
            {
                buckets[i] = ImmutableAvlNode<TRegistration>.Empty;
            }
            return buckets;
        }

        private void ReclassifyHashEntries(ImmutableAvlNode<TRegistration>[] newBuckets)
        {
            foreach (var avlTree in _buckets)
            {
                foreach (var node in avlTree)
                {
                    AddEntry(node.HashEntry, newBuckets);
                }
            }
        }

        private static void AddEntry(HashEntry<TypeKey, TRegistration> hashEntry, ImmutableAvlNode<TRegistration>[] buckets)
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
            ImmutableAvlNode<TRegistration>[] newBuckets;
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
            return new ImmutableRegistrationsContainer<TRegistration>(new PrimeNumberLinearStrategy<TRegistration>());
        }

        public static ImmutableRegistrationsContainer<TRegistration> CreateEmpty(IGrowBucketContainerStrategy<TRegistration> growContainerStrategy)
        {
            growContainerStrategy.MustNotBeNull(nameof(growContainerStrategy));

            return new ImmutableRegistrationsContainer<TRegistration>(growContainerStrategy);
        }

        public RegistrationEnumerator<TRegistration> GetRegistrationEnumeratorForType(Type type)
        {
            var targetBucket = _buckets[GetTargetBucketIndex(type, _buckets.Length)];
            // ReSharper disable once GenericEnumeratorNotDisposed
            return new RegistrationEnumerator<TRegistration>(type, targetBucket.GetEnumerator());
        }
    }
}