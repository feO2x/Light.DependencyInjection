using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct FastReadThreadSafeDictionaryOptions<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private IGrowBucketContainerStrategy<TKey, TValue> _growContainerStrategy;
        public static readonly IGrowBucketContainerStrategy<TKey, TValue> DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy<TKey, TValue>();

        public IGrowBucketContainerStrategy<TKey, TValue> GrowContainerStrategy
        {
            get { return _growContainerStrategy; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _growContainerStrategy = value;
            }
        }

        public static FastReadThreadSafeDictionaryOptions<TKey, TValue> Create()
        {
            return new FastReadThreadSafeDictionaryOptions<TKey, TValue>
                   {
                       _growContainerStrategy = DefaultGrowContainerStrategy
                   };
        }
    }
}