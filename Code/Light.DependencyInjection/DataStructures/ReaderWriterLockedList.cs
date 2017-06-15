using System;
using System.Collections;
using System.Collections.Generic;
using Light.DependencyInjection.Threading;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ReaderWriterLockedList<T> : IConcurrentList<T>, IDisposable
    {
        private readonly IEqualityComparer<T> _equalityComparer;
        private readonly IGrowArrayStrategy<T> _growArrayStrategy;
        private readonly IReaderWriterLock _lock;
        private int _count;
        private T[] _internalArray;

        public ReaderWriterLockedList() : this(new ReaderWriterLockedListOptions<T>()) { }

        public ReaderWriterLockedList(ReaderWriterLockedListOptions<T> options)
        {
            options.MustNotBeNull(nameof(options));

            _equalityComparer = options.EqualityComparer;
            _lock = options.Lock;
            _growArrayStrategy = options.GrowArrayStrategy;
            IReadOnlyList<T> initialItems = null;
            if (options.InitialItems != null)
                initialItems = options.InitialItems.AsReadOnlyList();
            _internalArray = _growArrayStrategy.CreateInitialArray(initialItems);
            _count = initialItems?.Count ?? 0;
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

            try
            {
                return InternalIndexOf(item) != -1;
            }
            finally
            {
                _lock.ExitReadLock();
            }
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

            try
            {
                return InternalIndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private int InternalIndexOf(T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if (_equalityComparer.Equals(_internalArray[i], item) == false)
                    continue;

                return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            index.MustNotBeLessThan(0, nameof(index));

            _lock.EnterWriteLock();
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
                _lock.ExitWriteLock();
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

            _internalArray = _growArrayStrategy.CreateLargerArrayFrom(_internalArray);
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
            private readonly IReaderWriterLock _lock;
            private T _current;
            private int _currentIndex;
            private bool _isLockAquired;

            public Enumerator(T[] internalArray, int count, IReaderWriterLock @lock)
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

        public T GetOrAdd(T item)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                var indexOfExistingItem = InternalIndexOf(item);
                if (indexOfExistingItem != -1)
                    return _internalArray[indexOfExistingItem];

                Add(item);
                return item;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void AddOrUpdate(T item)
        {
            throw new NotImplementedException();
        }
    }
}