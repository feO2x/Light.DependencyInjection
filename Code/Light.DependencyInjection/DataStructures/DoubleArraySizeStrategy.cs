using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class DoubleArraySizeStrategy<T> : IGrowArrayStrategy<T>
    {
        public const int DefaultCapacity = 4;
        public readonly int InitialCapacity;

        public DoubleArraySizeStrategy(int initialCapacity = DefaultCapacity)
        {
            initialCapacity.MustNotBeLessThan(1, nameof(initialCapacity));

            InitialCapacity = initialCapacity;
        }

        public T[] CreateInitialArray(IEnumerable<T> existingItems = null)
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (existingItems.IsNullOrEmpty())
                return new T[InitialCapacity];

            var existingItemsList = existingItems.AsList();
            var initialCapacity = InitialCapacity;
            checked
            {
                if (initialCapacity < existingItemsList.Count)
                    initialCapacity *= 2;
            }
            var initialArray = new T[initialCapacity];
            for (var i = 0; i < existingItemsList.Count; i++)
            {
                initialArray[i] = existingItemsList[i];
            }
            return initialArray;
            // ReSharper restore PossibleMultipleEnumeration
        }

        public T[] CreateLargerArrayFrom(T[] array)
        {
            array.MustNotBeNull();

            var newSize = array.Length;
            checked
            {
                newSize *= 2;
            }

            var newArray = new T[newSize];
            Array.Copy(array, newArray, array.Length);
            return newArray;
        }
    }
}