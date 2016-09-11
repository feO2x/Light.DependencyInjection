using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class PrimeNumberLinearStrategy<TKey, TValue> : BasePrimeNumbers, IGrowBucketContainerStrategy<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private readonly int _growthAdditionValue;
        private readonly int _maximalAvlTreeHeightBeforeResize;

        public PrimeNumberLinearStrategy(int maximalAvlTreeHeightBeforeResize = 5, int growthAdditionValue = 10)
        {
            _maximalAvlTreeHeightBeforeResize = maximalAvlTreeHeightBeforeResize;
            _growthAdditionValue = growthAdditionValue;
        }


        public int GetNumberOfBuckets(IReadOnlyList<ImmutableAvlNode<TKey, TValue>> existingBuckets)
        {
            if (existingBuckets == null || existingBuckets.Count == 0)
                return SelectedPrimeNumbers[0];

            for (var i = 0; i < existingBuckets.Count; i++)
            {
                if (existingBuckets[i].Height > _maximalAvlTreeHeightBeforeResize)
                    return CalculateNewCapacity(existingBuckets[i].Height);
            }
            return existingBuckets.Count;
        }

        private int CalculateNewCapacity(int existingCapacity)
        {
            foreach (var primeNumber in SelectedPrimeNumbers)
            {
                if (primeNumber > existingCapacity)
                    return primeNumber;
            }

            return existingCapacity + _growthAdditionValue;
        }
    }

    public abstract class BasePrimeNumbers
    {
        protected static readonly int[] SelectedPrimeNumbers =
            { 3, 5, 7, 11, 13, 17, 23, 29, 37, 41, 47, 53, 59, 67, 71, 79, 83, 89, 97, 101, 107, 113, 127, 131, 137, 139, 149, 157, 163, 167, 173, 179, 191, 197, 211, 223, 227, 233, 239, 251, 257, 269 };
    }
}