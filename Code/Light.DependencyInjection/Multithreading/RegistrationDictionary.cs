using System;
using System.Collections.Generic;
using System.Threading;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class RegistrationDictionary<TRegistration>
    {
        private readonly object _bucketContainerLock = new object();
        private ImmutableRegistrationsContainer<TRegistration> _bucketContainer;

        public RegistrationDictionary()
            : this(RegistrationDictionaryOptions<TRegistration>.Create()) { }

        public RegistrationDictionary(RegistrationDictionaryOptions<TRegistration> options)
        {
            _bucketContainer = ImmutableRegistrationsContainer<TRegistration>.CreateEmpty(options.GrowContainerStrategy);
        }

        public RegistrationDictionary(RegistrationDictionary<TRegistration> other)
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

        public IReadOnlyList<TRegistration> Registrations
        {
            get
            {
                var bucketContainer = Volatile.Read(ref _bucketContainer);
                return bucketContainer.Registrations;
            }
        }

        public bool TryGetValue(TypeKey typeKey, out TRegistration value)
        {
            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.TryFind(typeKey, out value);
        }

        public TRegistration GetOrAdd(TypeKey typeKey, Func<TRegistration> createRegistration)
        {
            TRegistration returnValue;
            GetOrAdd(typeKey, createRegistration, out returnValue);
            return returnValue;
        }

        public bool GetOrAdd(TypeKey typeKey, Func<TRegistration> createRegistration, out TRegistration registration)
        {
            createRegistration.MustNotBeNull(nameof(createRegistration));

            lock (_bucketContainerLock)
            {
                ImmutableRegistrationsContainer<TRegistration> newBucketContainer;
                if (_bucketContainer.GetOrAdd(typeKey, createRegistration, out registration, out newBucketContainer) == false)
                    return false;

                _bucketContainer = newBucketContainer;
                return true;
            }
        }

        public bool AddOrReplace(TypeKey typeKey, TRegistration registration)
        {
            bool result;
            lock (_bucketContainerLock)
            {
                ImmutableRegistrationsContainer<TRegistration> newBucketContainer;
                result = _bucketContainer.AddOrReplace(typeKey, registration, out newBucketContainer);
                _bucketContainer = newBucketContainer;
            }
            return result;
        }

        public RegistrationEnumerator<TRegistration> GetRegistrationEnumeratorForType(Type type)
        {
            var bucketContainer = Volatile.Read(ref _bucketContainer);
            return bucketContainer.GetRegistrationEnumeratorForType(type);
        }
    }
}