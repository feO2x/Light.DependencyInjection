using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class ImmutableList<T> : IEnumerable<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly T[] _array;

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

        public T this[int index] => _array[index];

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ImmutableList<T> Add(T item)
        {
            if (_array == null)
                return new ImmutableList<T>(new[] { item });

            var newEntries = new T[Count + 1];
            Array.Copy(_array, newEntries, Count);
            newEntries[Count] = item;
            return new ImmutableList<T>(newEntries);
        }

        public ImmutableListEnumerator GetEnumerator()
        {
            return new ImmutableListEnumerator(_array);
        }

        public ImmutableList<T> ReplaceAt(int index, T item)
        {
            var newArray = new T[Count];
            Array.Copy(_array, newArray, Count);
            newArray[index] = item;
            return new ImmutableList<T>(newArray);
        }

        public struct ImmutableListEnumerator : IEnumerator<T>
        {
            private readonly T[] _array;
            private T _current;
            private int _currentIndex;

            public ImmutableListEnumerator(T[] array)
            {
                array.MustNotBeNull(nameof(array));

                _array = array;
                _current = default(T);
                _currentIndex = -1;
            }

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

            public void Reset()
            {
                _currentIndex = -1;
                _current = default(T);
            }

            public T Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { }
        }
    }
}