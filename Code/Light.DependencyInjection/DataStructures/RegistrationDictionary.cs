using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a thread-safe dictionary for <see cref="Registration" /> instances that
    ///     allows non-locking read operations. Therefore the internal buckets (each bucket is an AVL tree) of the dictionary
    ///     are immutable. Registrations are identified by <see cref="TypeKey" /> instances.
    /// </summary>
    public sealed class RegistrationDictionary
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private ImmutableRegistrationBuckets _bucketContainer;

        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationDictionary" />.
        /// </summary>
        public RegistrationDictionary()
            : this(RegistrationDictionaryOptions.Create()) { }

        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationDictionary" /> with the specified options.
        /// </summary>
        /// <param name="options">The options that customize the behavior of the internal buckets.</param>
        public RegistrationDictionary(RegistrationDictionaryOptions options)
        {
            _bucketContainer = ImmutableRegistrationBuckets.CreateEmpty(options.GrowContainerStrategy);
        }

        /// <summary>
        ///     Intializes a new instance of <see cref="RegistrationDictionary" />, copying the internal buckets of the other instance.
        /// </summary>
        /// <param name="other">The other dictionary whose buckets are copied.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="other" /> is null.</exception>
        public RegistrationDictionary(RegistrationDictionary other)
        {
            other.MustNotBeNull(nameof(other));

            _bucketContainer = Volatile.Read(ref other._bucketContainer);
        }

        /// <summary>
        ///     Gets a new list containing all type keys of the dictionary.
        /// </summary>
        public IReadOnlyList<TypeKey> TypeKeys
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.TypeKeys;
            }
        }

        /// <summary>
        ///     Gets a new list containing all registrations of the dictionary.
        /// </summary>
        public IReadOnlyList<Registration> Registrations
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.Registrations;
            }
        }

        /// <summary>
        ///     Tries to retrieve the registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key that uniquely identifies the target registration.</param>
        /// <param name="registration">The registration to be retrieved.</param>
        /// <returns>True if the registration could be retrieved, else false.</returns>
        public bool TryGetValue(TypeKey typeKey, out Registration registration)
        {
            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.TryFind(typeKey, out registration);
        }

        /// <summary>
        ///     Gets or adds the registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key that uniquely identifies the target registration.</param>
        /// <param name="createRegistration">The delegate that creates the target registration if necessary.</param>
        /// <returns>The retrieved or created target registration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="createRegistration" /> is null.</exception>
        public Registration GetOrAdd(TypeKey typeKey, Func<Registration> createRegistration)
        {
            Registration returnValue;
            GetOrAdd(typeKey, createRegistration, out returnValue);
            return returnValue;
        }

        /// <summary>
        ///     Gets or adds the registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key that uniquely identifies the target registration.</param>
        /// <param name="createRegistration">The delegate that creates the target registration if necessary.</param>
        /// <param name="registration">The registration that was created or retrieved.</param>
        /// <returns>True if the registration was created, else false.</returns>
        public bool GetOrAdd(TypeKey typeKey, Func<Registration> createRegistration, out Registration registration)
        {
            createRegistration.MustNotBeNull(nameof(createRegistration));

            _semaphore.Wait();
            ImmutableRegistrationBuckets newBucketContainer;
            if (_bucketContainer.GetOrAdd(typeKey, createRegistration, out registration, out newBucketContainer) == false)
                return false;

            _bucketContainer = newBucketContainer;
            _semaphore.Release();
            return true;
        }

        /// <summary>
        ///     Adds or replaces the specified registration with the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying the registration.</param>
        /// <param name="registration">The registration to be added or replaced.</param>
        /// <returns>True if the registration was replaced, else false.</returns>
        public bool AddOrReplace(TypeKey typeKey, Registration registration)
        {
            bool result;

            _semaphore.Wait();
            ImmutableRegistrationBuckets newBucketContainer;
            result = _bucketContainer.AddOrReplace(typeKey, registration, out newBucketContainer);
            _bucketContainer = newBucketContainer;
            _semaphore.Release();
            return result;
        }

        /// <summary>
        ///     Gets an enumerator that can be used to iterate over all registrations whose mapping keys contain the specified type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.GetRegistrationEnumeratorForType(type);
        }
    }
}