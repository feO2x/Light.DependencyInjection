using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct SynchronizedDictionaryOptions<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private IGrowBucketContainerStrategy<TKey, TValue> _growContainerStrategy;
        public static readonly IGrowBucketContainerStrategy<TKey, TValue> DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy<TKey, TValue>();
        private IEqualityComparer<TKey> _keyComparer;

        public IGrowBucketContainerStrategy<TKey, TValue> GrowContainerStrategy
        {
            get { return _growContainerStrategy; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _growContainerStrategy = value;
            }
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _keyComparer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _keyComparer = value;
            }
        }

        public static SynchronizedDictionaryOptions<TKey, TValue> Create()
        {
            return new SynchronizedDictionaryOptions<TKey, TValue>
                   {
                       _growContainerStrategy = DefaultGrowContainerStrategy,
                       _keyComparer = EqualityComparer<TKey>.Default
                   };
        }
    }
}