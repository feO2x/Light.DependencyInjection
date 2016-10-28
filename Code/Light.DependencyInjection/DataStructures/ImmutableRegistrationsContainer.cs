using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ImmutableRegistrationsContainer
    {
        private readonly ImmutableAvlNode<Registration>[] _buckets;
        private readonly IGrowBucketContainerStrategy _growContainerStrategy;
        private readonly Lazy<IReadOnlyList<TypeKey>> _lazyKeys;
        private readonly Lazy<IReadOnlyList<Registration>> _lazyValues;
        public readonly int Count;
        public readonly bool IsEmpty;
        public readonly int NumberOfBuckets;

        private ImmutableRegistrationsContainer(IGrowBucketContainerStrategy growContainerStrategy)
        {
            _growContainerStrategy = growContainerStrategy;
            Count = 0;
            NumberOfBuckets = 0;
            IsEmpty = true;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<Registration>>(CreateValues);
        }

        private ImmutableRegistrationsContainer(ImmutableAvlNode<Registration>[] buckets,
                                                int count,
                                                IGrowBucketContainerStrategy growContainerStrategy)
        {
            _buckets = buckets;
            NumberOfBuckets = buckets.Length;
            Count = count;
            _growContainerStrategy = growContainerStrategy;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<Registration>>(CreateValues);
        }

        public IReadOnlyList<TypeKey> TypeKeys => _lazyKeys.Value;

        public IReadOnlyList<Registration> Registrations => _lazyValues.Value;

        public ImmutableAvlNode<Registration> this[int index] => _buckets[index];

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

        private IReadOnlyList<Registration> CreateValues()
        {
            var values = new Registration[Count];
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

        private ImmutableAvlNode<Registration>[] CreateNewBuckets(bool tryGrow)
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

        private static ImmutableAvlNode<Registration>[] InitializeBuckets(int numberOfBuckets)
        {
            var buckets = new ImmutableAvlNode<Registration>[numberOfBuckets];
            for (var i = 0; i < numberOfBuckets; i++)
            {
                buckets[i] = ImmutableAvlNode<Registration>.Empty;
            }
            return buckets;
        }

        private void ReclassifyHashEntries(ImmutableAvlNode<Registration>[] newBuckets)
        {
            foreach (var avlTree in _buckets)
            {
                foreach (var node in avlTree)
                {
                    AddEntry(node.HashEntry, newBuckets);
                }
            }
        }

        private static void AddEntry(HashEntry<TypeKey, Registration> hashEntry, ImmutableAvlNode<Registration>[] buckets)
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

        public bool TryFind(TypeKey typeKey, out Registration registration)
        {
            if (IsEmpty)
            {
                registration = null;
                return false;
            }

            var targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
            return _buckets[targetBucketIndex].TryFind(typeKey.HashCode, typeKey, out registration);
        }

        public bool GetOrAdd(TypeKey typeKey,
                             Func<Registration> createRegistration,
                             out Registration registration,
                             out ImmutableRegistrationsContainer newBucketContainer)
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
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TypeKey, Registration>(typeKey.HashCode, typeKey, registration));
            newBucketContainer = new ImmutableRegistrationsContainer(newBuckets, Count + 1, _growContainerStrategy);
            return true;
        }

        public bool AddOrReplace(TypeKey typeKey,
                                 Registration registration,
                                 out ImmutableRegistrationsContainer newBucketContainer)
        {
            ImmutableAvlNode<Registration>[] newBuckets;
            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
                var targetNode = _buckets[targetBucketIndex];
                if (targetNode.TryFind(typeKey.HashCode, typeKey))
                {
                    newBuckets = CreateNewBuckets(false);
                    newBuckets[targetBucketIndex] = targetNode.Replace(new HashEntry<TypeKey, Registration>(typeKey.HashCode, typeKey, registration));
                    newBucketContainer = new ImmutableRegistrationsContainer(newBuckets, Count, _growContainerStrategy);
                    return false;
                }
            }
            newBuckets = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(typeKey, newBuckets.Length);
            newBuckets[targetBucketIndex] = newBuckets[targetBucketIndex].Add(new HashEntry<TypeKey, Registration>(typeKey.HashCode, typeKey, registration));
            newBucketContainer = new ImmutableRegistrationsContainer(newBuckets, Count + 1, _growContainerStrategy);
            return true;
        }

        public static ImmutableRegistrationsContainer CreateEmpty()
        {
            return new ImmutableRegistrationsContainer(new PrimeNumberLinearStrategy());
        }

        public static ImmutableRegistrationsContainer CreateEmpty(IGrowBucketContainerStrategy growContainerStrategy)
        {
            growContainerStrategy.MustNotBeNull(nameof(growContainerStrategy));

            return new ImmutableRegistrationsContainer(growContainerStrategy);
        }

        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            var targetBucket = _buckets[GetTargetBucketIndex(type, _buckets.Length)];
            // ReSharper disable once GenericEnumeratorNotDisposed
            return new RegistrationEnumerator(type, targetBucket.GetEnumerator());
        }
    }
}