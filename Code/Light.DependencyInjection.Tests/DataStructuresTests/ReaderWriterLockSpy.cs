using Light.DependencyInjection.Threading;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    public sealed class ReaderWriterLockSpy : IReaderWriterLock
    {
        private int _disposeCount;
        private int _enterReadLockCount;
        private int _enterWriteLockCount;
        private int _exitReadLockCount;
        private int _exitWriteLockCount;
        private int _enterUpgradeableReadLockCount;
        private int _exitUpgradeableReadLockCount;

        public int EnterReadLockCount => _enterReadLockCount;
        public int ExitReadLockCount => _exitReadLockCount;
        public int EnterWriteLockCount => _enterWriteLockCount;
        public int ExitWriteLockCount => _exitWriteLockCount;
        public int EnterUpgradeableReadLockCount => _enterUpgradeableReadLockCount;
        public int ExitUpgradeableReadLockCount => _exitUpgradeableReadLockCount;

        public int DisposeCount => _disposeCount;

        public void EnterReadLock()
        {
            _enterReadLockCount++;
        }

        public void ExitReadLock()
        {
            _exitReadLockCount++;
        }

        public void EnterUpgradeableReadLock()
        {
            _enterUpgradeableReadLockCount++;
        }

        public void ExitUpgradeableReadLock()
        {
            _exitUpgradeableReadLockCount++;
        }

        public void EnterWriteLock()
        {
            _enterWriteLockCount++;
        }

        public void ExitWriteLock()
        {
            _exitWriteLockCount++;
        }

        public void Dispose()
        {
            _disposeCount++;
        }
    }
}