using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ReaderWriterLockedList<T> : IList<T>, IDisposable
    {
        public const int DefaultCapacity = 4;
        private readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _count;
        private T[] _internalArray;

        public ReaderWriterLockedList(int initialCapacity = DefaultCapacity)
        {
            initialCapacity.MustNotBeLessThan(0);

            _internalArray = new T[initialCapacity];
        }

        public ReaderWriterLockedList(IEnumerable<T> initialItems)
        {
            var initialItemsList = initialItems.AsList();
            var targetCapacity = 2;
            while (targetCapacity < initialItemsList.Count)
                targetCapacity *= 2;

            _internalArray = new T[targetCapacity];
            var currentIndex = 0;
            foreach (var item in initialItemsList)
            {
                _internalArray[currentIndex++] = item;
            }
            _count = initialItemsList.Count;
        }

        public int Capacity
        {
            get
            {
                _lock.EnterReadLock();
                var capacity = _internalArray.Length;
                _lock.ExitReadLock();
                return capacity;
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                ExchangeInternalArrayWithLargerOneIfNecessary();

                _internalArray[_count++] = item;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();

            for (var i = 0; i < _count; i++)
            {
                _internalArray[i] = default(T);
            }
            _count = 0;

            _lock.ExitWriteLock();
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();

            var result = false;
            for (var i = 0; i < _count; i++)
            {
                if (_equalityComparer.Equals(item, _internalArray[i]) == false)
                    continue;

                result = true;
                break;
            }

            _lock.ExitReadLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array.MustNotBeNull(nameof(arrayIndex));
            arrayIndex.MustNotBeLessThan(0, nameof(arrayIndex));

            _lock.EnterReadLock();
            try
            {
                var numberOfSlotsInTargetArray = array.Length - arrayIndex;
                if (numberOfSlotsInTargetArray < _count)
                    throw new ArgumentException($"The target array only has {numberOfSlotsInTargetArray} slots left for copying (length: {array.Length}, startingIndex: {arrayIndex}), but ConcurrentList contains actually {_count} items.");

                Array.Copy(_internalArray, 0, array, arrayIndex, _count);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();

            var result = false;
            for (var i = 0; i < _count; i++)
            {
                if (_equalityComparer.Equals(_internalArray[i], item) == false)
                    continue;

                InternalRemoveAt(i);
                result = true;
                break;
            }

            _lock.ExitWriteLock();
            return result;
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                var count = _count;
                _lock.ExitReadLock();
                return count;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            _lock.EnterReadLock();

            var targetIndex = -1;
            for (var i = 0; i < _count; i++)
            {
                if (_equalityComparer.Equals(_internalArray[i], item) == false)
                    continue;

                targetIndex = i;
                break;
            }
            _lock.ExitReadLock();
            return targetIndex;
        }

        public void Insert(int index, T item)
        {
            index.MustNotBeLessThan(0, nameof(index));

            _lock.EnterReadLock();
            try
            {
                index.MustNotBeGreaterThan(_count, nameof(index));

                ExchangeInternalArrayWithLargerOneIfNecessary();

                for (var i = _count; i > index; i--)
                {
                    _internalArray[i] = _internalArray[i - 1];
                }
                _internalArray[index] = item;
                _count++;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void RemoveAt(int index)
        {
            index.MustNotBeLessThan(0, nameof(index));

            _lock.EnterWriteLock();
            try
            {
                index.MustNotBeGreaterThanOrEqualTo(_count, nameof(index));

                InternalRemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                index.MustNotBeLessThan(0, exception: () => new IndexOutOfRangeException("index must not be less than zero."));
                _lock.EnterReadLock();

                try
                {
                    index.MustNotBeGreaterThanOrEqualTo(_count, exception: () => new IndexOutOfRangeException("index must not be greater than Count."));

                    return _internalArray[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                index.MustNotBeLessThan(0, exception: () => new IndexOutOfRangeException("index must not be less than zero."));

                _lock.EnterWriteLock();
                try
                {
                    index.MustNotBeGreaterThanOrEqualTo(_count, exception: () => new IndexOutOfRangeException("index must not be greater than Count."));

                    _internalArray[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_internalArray, _count, _lock);
        }

        private void ExchangeInternalArrayWithLargerOneIfNecessary()
        {
            if (_count + 1 < _internalArray.Length)
                return;

            var newSize = _internalArray.Length * 2;
            var newArray = new T[newSize];
            Array.Copy(_internalArray, newArray, _internalArray.Length);
            _internalArray = newArray;
        }

        private void InternalRemoveAt(int index)
        {
            for (var i = index + 1; i < _count; i++)
            {
                _internalArray[i - 1] = _internalArray[i];
            }
            _internalArray[--_count] = default(T);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _internalArray;
            private readonly int _count;
            private readonly ReaderWriterLockSlim _lock;
            private T _current;
            private int _currentIndex;
            private bool _isLockAquired;

            public Enumerator(T[] internalArray, int count, ReaderWriterLockSlim @lock)
            {
                internalArray.MustNotBeNull();
                @lock.MustNotBeNull();
                count.MustNotBeLessThan(0);
                count.MustNotBeGreaterThan(internalArray.Length);

                _internalArray = internalArray;
                _count = count;
                _lock = @lock;

                _current = default(T);
                _currentIndex = -1;
                _isLockAquired = false;
            }

            public bool MoveNext()
            {
                if (_currentIndex + 1 == _count)
                {
                    ReleaseLockIfNecessary();
                    _current = default(T);
                    return false;
                }

                AquireLockIfNecessary();
                _current = _internalArray[++_currentIndex];
                return true;
            }

            private void AquireLockIfNecessary()
            {
                if (_isLockAquired) return;

                _lock.EnterReadLock();
                _isLockAquired = true;
            }

            private void ReleaseLockIfNecessary()
            {
                if (_isLockAquired == false) return;

                _lock.ExitReadLock();
                _isLockAquired = false;
            }

            public void Reset()
            {
                ReleaseLockIfNecessary();
                _currentIndex = -1;
                _current = default(T);
            }

            public T Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
                if (_isLockAquired)
                {
                    _lock.ExitReadLock();
                    _isLockAquired = false;
                }
            }
        }
    }
}