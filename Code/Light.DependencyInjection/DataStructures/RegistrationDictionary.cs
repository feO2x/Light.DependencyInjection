using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class RegistrationDictionary
    {
        private readonly object _bucketContainerLock = new object();
        private ImmutableRegistrationBuckets _bucketContainer;

        public RegistrationDictionary()
            : this(RegistrationDictionaryOptions.Create()) { }

        public RegistrationDictionary(RegistrationDictionaryOptions options)
        {
            _bucketContainer = ImmutableRegistrationBuckets.CreateEmpty(options.GrowContainerStrategy);
        }

        public RegistrationDictionary(RegistrationDictionary other)
        {
            _bucketContainer = Volatile.Read(ref other._bucketContainer);
        }

        public IReadOnlyList<TypeKey> TypeKeys
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.TypeKeys;
            }
        }

        public IReadOnlyList<Registration> Registrations
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.Registrations;
            }
        }

        public bool TryGetValue(TypeKey typeKey, out Registration value)
        {
            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.TryFind(typeKey, out value);
        }

        public Registration GetOrAdd(TypeKey typeKey, Func<Registration> createRegistration)
        {
            Registration returnValue;
            GetOrAdd(typeKey, createRegistration, out returnValue);
            return returnValue;
        }

        public bool GetOrAdd(TypeKey typeKey, Func<Registration> createRegistration, out Registration registration)
        {
            createRegistration.MustNotBeNull(nameof(createRegistration));

            lock (_bucketContainerLock)
            {
                ImmutableRegistrationBuckets newBucketContainer;
                if (_bucketContainer.GetOrAdd(typeKey, createRegistration, out registration, out newBucketContainer) == false)
                    return false;

                _bucketContainer = newBucketContainer;
                return true;
            }
        }

        public bool AddOrReplace(TypeKey typeKey, Registration registration)
        {
            bool result;
            lock (_bucketContainerLock)
            {
                ImmutableRegistrationBuckets newBucketContainer;
                result = _bucketContainer.AddOrReplace(typeKey, registration, out newBucketContainer);
                _bucketContainer = newBucketContainer;
            }
            return result;
        }

        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.GetRegistrationEnumeratorForType(type);
        }
    }
}