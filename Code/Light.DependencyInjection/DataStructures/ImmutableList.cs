using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents an immutable collection. Creates new instances when items are added.
    /// </summary>
    /// <typeparam name="T">The item type of the collection.</typeparam>
    public sealed class ImmutableList<T> : IEnumerable<T>
    {
        /// <summary>
        ///     Gets an empty immutable list.
        /// </summary>
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly T[] _array;

        /// <summary>
        ///     Gets the number of items in the collection.
        /// </summary>
        public readonly int Count;

        private ImmutableList()
        {
            Count = 0;
            _array = new T[0];
        }

        private ImmutableList(T[] array)
        {
            _array = array;
            Count = array.Length;
        }

        /// <summary>
        ///     Gets the item at the specified index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is less than zero or greater or equal than <see cref="Count" />.</exception>
        public T this[int index] => _array[index];

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Creates a new immutable list with the specified item added to the back.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>A new instance of immutable list.</returns>
        public ImmutableList<T> Add(T item)
        {
            if (_array == null)
                return new ImmutableList<T>(new[] { item });

            var newEntries = new T[Count + 1];
            Array.Copy(_array, newEntries, Count);
            newEntries[Count] = item;
            return new ImmutableList<T>(newEntries);
        }

        /// <summary>
        ///     Gets the enumerator for this list.
        /// </summary>
        public ImmutableListEnumerator GetEnumerator()
        {
            return new ImmutableListEnumerator(_array);
        }

        /// <summary>
        ///     Creates a new immutable list with the specified item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to be replaced.</param>
        /// <param name="item">The item to be inserted.</param>
        /// <returns>A new instance of immutable list.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is less than zero or greater or equal than <see cref="Count" />.</exception>
        public ImmutableList<T> ReplaceAt(int index, T item)
        {
            var newArray = new T[Count];
            Array.Copy(_array, newArray, Count);
            newArray[index] = item;
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        ///     Represents a structure that can enumerate through an array.
        /// </summary>
        public struct ImmutableListEnumerator : IEnumerator<T>
        {
            private readonly T[] _array;
            private T _current;
            private int _currentIndex;

            /// <summary>
            ///     Initializes a new instance of <see cref="ImmutableListEnumerator" />.
            /// </summary>
            /// <param name="array">The array that will be iterated.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="array" /> is null.</exception>
            public ImmutableListEnumerator(T[] array)
            {
                array.MustNotBeNull(nameof(array));

                _array = array;
                _current = default(T);
                _currentIndex = -1;
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (++_currentIndex == _array.Length)
                {
                    --_currentIndex;
                    _current = default(T);
                    return false;
                }

                _current = _array[_currentIndex];
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _currentIndex = -1;
                _current = default(T);
            }

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            public T Current => _current;

            object IEnumerator.Current => _current;

            /// <summary>
            ///     Does nothing because this enumerator has no resources that must be disposed.
            /// </summary>
            public void Dispose() { }
        }
    }
}