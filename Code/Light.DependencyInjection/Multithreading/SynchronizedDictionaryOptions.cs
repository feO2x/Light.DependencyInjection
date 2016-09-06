using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct SynchronizedDictionaryOptions<TKey>
    {
        public const int DefaultInitialNumberOfBuckets = 31;
        public const int DefaultBucketCapacity = 10;
        public const float DefaultGrowthRate = 2f;

        private IEqualityComparer<TKey> _keyComparer;
        private int _initialNumberOfBuckets;
        private int _bucketCapacity;
        private float _growthRate;

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _keyComparer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _keyComparer = value;
            }
        }

        public int InitialNumberOfBuckets
        {
            get { return _initialNumberOfBuckets; }
            set
            {
                value.MustNotBeLessThan(2, nameof(value));
                _initialNumberOfBuckets = value;
            }
        }

        public int BucketCapacity
        {
            get { return _bucketCapacity; }
            set
            {
                value.MustNotBeLessThan(2, nameof(value));
                _bucketCapacity = value;
            }
        }

        public float GrowthRate
        {
            get { return _growthRate; }
            set
            {
                value.MustBeGreaterThan(1f, nameof(value));
                _growthRate = value;
            }
        }

        public static SynchronizedDictionaryOptions<TKey> Create()
        {
            return new SynchronizedDictionaryOptions<TKey>
                   {
                       _initialNumberOfBuckets = DefaultInitialNumberOfBuckets,
                       _bucketCapacity = DefaultBucketCapacity,
                       _keyComparer = EqualityComparer<TKey>.Default,
                       _growthRate = DefaultGrowthRate
                   };
        }
    }
}