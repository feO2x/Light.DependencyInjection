using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a collection of immutable AVL trees that are used as buckets in the <see cref="RegistrationDictionary" />.
    ///     This bucket container is immutable, too, thus instead of changing its state, new instances will be returned
    ///     to guarantee lock-free read operations on the dictionary.
    /// </summary>
    public sealed class ImmutableRegistrationBuckets
    {
        private readonly ImmutableAvlNode<Registration>[] _buckets;
        private readonly IGrowBucketsStrategy _growContainerStrategy;
        private readonly Lazy<IReadOnlyList<TypeKey>> _lazyKeys;
        private readonly Lazy<IReadOnlyList<Registration>> _lazyValues;

        /// <summary>
        ///     Gets the value indicating whether the bucket container is empty.
        /// </summary>
        public readonly bool IsEmpty;

        /// <summary>
        ///     Gets the number of buckets that are currently allocated.
        /// </summary>
        public readonly int NumberOfBuckets;

        /// <summary>
        ///     Gets the total number of entries of all AVL trees managed in the buckets.
        /// </summary>
        public readonly int NumberOfEntries;

        private ImmutableRegistrationBuckets(IGrowBucketsStrategy growContainerStrategy)
        {
            _growContainerStrategy = growContainerStrategy;
            NumberOfEntries = 0;
            NumberOfBuckets = 0;
            IsEmpty = true;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<Registration>>(CreateValues);
        }

        private ImmutableRegistrationBuckets(ImmutableAvlNode<Registration>[] buckets,
                                             int numberOfEntries,
                                             IGrowBucketsStrategy growContainerStrategy)
        {
            _buckets = buckets;
            NumberOfBuckets = buckets.Length;
            NumberOfEntries = numberOfEntries;
            _growContainerStrategy = growContainerStrategy;
            _lazyKeys = new Lazy<IReadOnlyList<TypeKey>>(CreateKeys);
            _lazyValues = new Lazy<IReadOnlyList<Registration>>(CreateValues);
        }

        /// <summary>
        ///     Gets the list of type keys residing in the bucket container.
        /// </summary>
        public IReadOnlyList<TypeKey> TypeKeys => _lazyKeys.Value;

        /// <summary>
        ///     Gets the list of registrations residing in the bucket container.
        /// </summary>
        public IReadOnlyList<Registration> Registrations => _lazyValues.Value;

        /// <summary>
        ///     Returns the immutable AVL tree at the specified index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index" /> is less than zero or greater than <see cref="NumberOfBuckets" />.</exception>
        public ImmutableAvlNode<Registration> this[int index] => _buckets[index];

        private IReadOnlyList<TypeKey> CreateKeys()
        {
            var keys = new TypeKey[NumberOfEntries];
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
            var values = new Registration[NumberOfEntries];
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
            var numberOfBuckets = tryGrow ? _growContainerStrategy.GetNumberOfBuckets(this) : _buckets.Length;
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

        /// <summary>
        ///     Tries to retrieve the registration with the given type key from the buckets.
        /// </summary>
        /// <param name="typeKey">The type key that identifies the registration.</param>
        /// <param name="registration">The retrieved registration.</param>
        /// <returns>True if the registration could be retrieved, else false.</returns>
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

        /// <summary>
        ///     Gets or adds a registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the target registration.</param>
        /// <param name="createRegistration">The delegate that will create the registration instance if necessary.</param>
        /// <param name="registration">The retrieved or created registration.</param>
        /// <param name="newBucketContainer">A new instance of <see cref="ImmutableRegistrationBuckets" /> with the added registration when the type key was not present, else null.</param>
        /// <returns>True if the registration was created using <paramref name="createRegistration" /> and inserted into a new instance of <see cref="ImmutableRegistrationBuckets" />, else false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createRegistration" /> is null.</exception>
        public bool GetOrAdd(TypeKey typeKey,
                             Func<Registration> createRegistration,
                             out Registration registration,
                             out ImmutableRegistrationBuckets newBucketContainer)
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
            newBucketContainer = new ImmutableRegistrationBuckets(newBuckets, NumberOfEntries + 1, _growContainerStrategy);
            return true;
        }

        /// <summary>
        ///     Adds or replaces a registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the target registration.</param>
        /// <param name="registration">The registration that should be added or replaced.</param>
        /// <param name="newBuckets">A new instance of <see cref="ImmutableRegistrationBuckets" /> with the added or replaced registration.</param>
        /// <returns>True if the registration was replaced, else false.</returns>
        public bool AddOrReplace(TypeKey typeKey,
                                 Registration registration,
                                 out ImmutableRegistrationBuckets newBuckets)
        {
            ImmutableAvlNode<Registration>[] newBucketsArray;
            int targetBucketIndex;
            if (IsEmpty == false)
            {
                targetBucketIndex = GetTargetBucketIndex(typeKey, _buckets.Length);
                var targetNode = _buckets[targetBucketIndex];
                if (targetNode.TryFind(typeKey.HashCode, typeKey))
                {
                    newBucketsArray = CreateNewBuckets(false);
                    newBucketsArray[targetBucketIndex] = targetNode.Replace(new HashEntry<TypeKey, Registration>(typeKey.HashCode, typeKey, registration));
                    newBuckets = new ImmutableRegistrationBuckets(newBucketsArray, NumberOfEntries, _growContainerStrategy);
                    return false;
                }
            }
            newBucketsArray = CreateNewBuckets(true);
            targetBucketIndex = GetTargetBucketIndex(typeKey, newBucketsArray.Length);
            newBucketsArray[targetBucketIndex] = newBucketsArray[targetBucketIndex].Add(new HashEntry<TypeKey, Registration>(typeKey.HashCode, typeKey, registration));
            newBuckets = new ImmutableRegistrationBuckets(newBucketsArray, NumberOfEntries + 1, _growContainerStrategy);
            return true;
        }

        /// <summary>
        ///     Creates an empty <see cref="ImmutableRegistrationBuckets" /> instance with a <see cref="PrimeNumberLinearStrategy" /> to grow the buckets.
        /// </summary>
        public static ImmutableRegistrationBuckets CreateEmpty()
        {
            return new ImmutableRegistrationBuckets(new PrimeNumberLinearStrategy());
        }

        /// <summary>
        ///     Creates an empty <see cref="ImmutableRegistrationBuckets" /> instance with the specified <paramref name="growContainerStrategy" />.
        /// </summary>
        /// <param name="growContainerStrategy">The strategy deciding how the buckets will grow in size.</param>
        /// <returns>A new empty <see cref="ImmutableRegistrationBuckets" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="growContainerStrategy" /> is null.</exception>
        public static ImmutableRegistrationBuckets CreateEmpty(IGrowBucketsStrategy growContainerStrategy)
        {
            growContainerStrategy.MustNotBeNull(nameof(growContainerStrategy));

            return new ImmutableRegistrationBuckets(growContainerStrategy);
        }

        /// <summary>
        ///     Gets an enumerator that iterates over all registrations that are mapped to the specified type.
        /// </summary>
        /// <param name="type">The type whose registrations (named and unnamed) should be retrieved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var targetBucket = _buckets[GetTargetBucketIndex(type, _buckets.Length)];
            // ReSharper disable once GenericEnumeratorNotDisposed
            return new RegistrationEnumerator(type, targetBucket.GetEnumerator());
        }
    }
}