using System;
using System.Collections.Generic;
using Light.GuardClauses;

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

        public T[] CreateInitialArray(IReadOnlyList<T> existingItems = null)
        {
            if (existingItems.IsNullOrEmpty())
                return new T[InitialCapacity];

            var initialCapacity = InitialCapacity;
            checked
            {
                // ReSharper disable once PossibleNullReferenceException
                if (initialCapacity < existingItems.Count)
                    initialCapacity *= 2;
            }
            var initialArray = new T[initialCapacity];
            for (var i = 0; i < existingItems.Count; i++)
            {
                initialArray[i] = existingItems[i];
            }
            return initialArray;
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