using System.Collections.Generic;
using Light.DependencyInjection.Threading;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ReaderWriterLockedListOptions<T>
    {
        private IEqualityComparer<T> _equalityComparer;
        private IGrowArrayStrategy<T> _growArrayStrategy;
        private IReaderWriterLock _lock;

        public ReaderWriterLockedListOptions(IReaderWriterLock @lock = null,
                                             IGrowArrayStrategy<T> growArrayStrategy = null,
                                             IEqualityComparer<T> equalityComparer = null,
                                             IEnumerable<T> initialItems = null)
        {
            _lock = @lock ?? new ReaderWriterLockSlim();
            _growArrayStrategy = growArrayStrategy ?? new DoubleArraySizeStrategy<T>();
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            InitialItems = initialItems;
        }

        public IReaderWriterLock Lock
        {
            get => _lock;
            set
            {
                value.MustNotBeNull();
                _lock = value;
            }
        }

        public IEnumerable<T> InitialItems { get; set; }

        public IGrowArrayStrategy<T> GrowArrayStrategy
        {
            get => _growArrayStrategy;
            set
            {
                value.MustNotBeNull();
                _growArrayStrategy = value;
            }
        }

        public IEqualityComparer<T> EqualityComparer
        {
            get => _equalityComparer;
            set
            {
                value.MustNotBeNull();
                _equalityComparer = value;
            }
        }
    }
}