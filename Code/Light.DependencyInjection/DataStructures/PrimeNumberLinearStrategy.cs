using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a <see cref="IGrowBucketsStrategy" /> that uses selected prime numbers and a maximal tree height to determine the number of buckets.
    ///     The tree height is ignored when the corresponding AVL tree only contains entries of the same type.
    /// </summary>
    public sealed class PrimeNumberLinearStrategy : IGrowBucketsStrategy
    {
        // ReSharper disable once InconsistentNaming
        private static readonly int[] _selectedPrimeNumbers =
            { 3, 5, 7, 11, 13, 17, 23, 29, 37, 41, 47, 53, 59, 67, 71, 79, 83, 89, 97, 101, 107, 113, 127, 131, 137, 139, 149, 157, 163, 167, 173, 179, 191, 197, 211, 223, 227, 233, 239, 251, 257, 269 };

        private readonly int _growthAdditionValue;
        private readonly int _maximalAvlTreeHeightBeforeResize;

        /// <summary>
        ///     Initializes a new instance of <see cref="PrimeNumberLinearStrategy" />.
        /// </summary>
        /// <param name="maximalAvlTreeHeightBeforeResize">The maximal AVL tree height that is allowed before a resize is initiated.</param>
        /// <param name="growthAdditionValue">The value used to determine the new capacity if the <see cref="SelectedPrimeNumbers" /> are exhausted.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maximalAvlTreeHeightBeforeResize" /> is less than 2 or <paramref name="growthAdditionValue" /> is less than 5.</exception>
        public PrimeNumberLinearStrategy(int maximalAvlTreeHeightBeforeResize = 5, int growthAdditionValue = 10)
        {
            maximalAvlTreeHeightBeforeResize.MustNotBeLessThan(2, nameof(maximalAvlTreeHeightBeforeResize));
            growthAdditionValue.MustNotBeLessThan(5, nameof(growthAdditionValue));

            _maximalAvlTreeHeightBeforeResize = maximalAvlTreeHeightBeforeResize;
            _growthAdditionValue = growthAdditionValue;
        }

        /// <summary>
        ///     Gets the list of prime numbers that were selected as valid bucket sizes.
        /// </summary>
        /// <remarks>
        ///     The prime numbers are:
        ///     3, 5, 7, 11, 13, 17, 23, 29, 37, 41, 47, 53, 59, 67, 71, 79, 83, 89, 97, 101, 107, 113, 127, 131, 137, 139, 149, 157, 163, 167, 173, 179, 191, 197, 211, 223, 227, 233, 239, 251, 257, and 269
        /// </remarks>
        public static IReadOnlyList<int> SelectedPrimeNumbers => _selectedPrimeNumbers;

        /// <inheritdoc />
        public int GetNumberOfBuckets(ImmutableRegistrationBuckets existingBuckets)
        {
            existingBuckets.MustNotBeNull(nameof(existingBuckets));

            if (existingBuckets.NumberOfEntries == 0)
                return _selectedPrimeNumbers[0];

            for (var i = 0; i < existingBuckets.NumberOfBuckets; i++)
            {
                var avlTree = existingBuckets[i];
                if (avlTree.Height <= _maximalAvlTreeHeightBeforeResize)
                    continue;

                if (CheckIfAvlTreeContainsDifferentTypes(avlTree))
                    return CalculateNewCapacity(avlTree.Height);
            }
            return existingBuckets.NumberOfEntries;
        }

        private static bool CheckIfAvlTreeContainsDifferentTypes(ImmutableAvlNode<Registration> avlTree)
        {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = avlTree.GetEnumerator();

            // The initial call to enumerator.MoveNext will not return false because the avlTree has at least height 2.
            enumerator.MoveNext();
            var initialType = enumerator.Current.HashEntry.Key.Type;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.HashEntry.Key.Type != initialType)
                    return true;
            }

            return false;
        }

        private int CalculateNewCapacity(int existingCapacity)
        {
            foreach (var primeNumber in _selectedPrimeNumbers)
            {
                if (primeNumber > existingCapacity)
                    return primeNumber;
            }

            return existingCapacity + _growthAdditionValue;
        }
    }
}