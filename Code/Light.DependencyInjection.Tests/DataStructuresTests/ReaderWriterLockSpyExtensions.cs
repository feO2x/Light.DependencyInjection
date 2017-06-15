using FluentAssertions;
using Xunit.Sdk;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    public static class ReaderWriterLockSpyExtensions
    {
        public static ReaderWriterLockSpy MustHaveUsedReadLockExactlyOnce(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterReadLockCount != 1 || spy.ExitReadLockCount != 1)
                throw new XunitException($"The lock should be used to enter and exit in read mode exactly once, but was actually entered {spy.EnterReadLockCount} times and exited {spy.ExitReadLockCount} times.");

            return spy;
        }

        public static ReaderWriterLockSpy MustHaveUsedWriteLockExactlyOnce(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterWriteLockCount != 1 || spy.ExitWriteLockCount != 1)
                throw new XunitException($"The lock should be used to enter and exit in write mode exactly once, but was actually entered {spy.EnterWriteLockCount} times and exited {spy.ExitWriteLockCount} times.");

            return spy;
        }

        public static ReaderWriterLockSpy MustHaveBeenDisposed(this ReaderWriterLockSpy spy)
        {
            spy.DisposeCount.Should().BeGreaterOrEqualTo(1);
            return spy;
        }

        public static ReaderWriterLockSpy MustHaveUsedUpgradeableReadLockExactlyOnce(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterUpgradeableReadLockCount != 1 || spy.ExitUpgradeableReadLockCount != 1)
                throw new XunitException($"The lock should be used to enter and exit in upgradeable read mode exactly once, but was actually entered {spy.EnterUpgradeableReadLockCount} times and exited {spy.ExitUpgradeableReadLockCount} times.");

            return spy;
        }

        public static ReaderWriterLockSpy MustNotHaveUsedWriteLock(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterWriteLockCount != 0 || spy.ExitWriteLockCount != 0)
                throw new XunitException($"The lock should not have been used in write mode, but was actually entered {spy.EnterWriteLockCount} times and exited {spy.ExitWriteLockCount} times.");

            return spy;
        }
    }
}