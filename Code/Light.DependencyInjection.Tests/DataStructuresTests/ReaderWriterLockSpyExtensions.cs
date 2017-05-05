using FluentAssertions;
using Xunit.Sdk;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    public static class ReaderWriterLockSpyExtensions
    {
        public static void MustHaveUsedReadLockExactlyOnce(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterReadLockCount != 1 || spy.ExitReadLockCount != 1)
                throw new XunitException($"The lock should be used to enter and exit in read mode exactly once, but was actually entered {spy.EnterReadLockCount} times and exited {spy.ExitReadLockCount} times.");
        }

        public static void MustHaveUsedWriteLockExactlyOnce(this ReaderWriterLockSpy spy)
        {
            if (spy.EnterWriteLockCount != 1 || spy.ExitWriteLockCount != 1)
                throw new XunitException($"The lock should be used to enter and exit in write mode exactly once, but was actually entered {spy.EnterWriteLockCount} times and exited {spy.ExitWriteLockCount} times.");
        }

        public static void MustHaveBeenDisposed(this ReaderWriterLockSpy spy)
        {
            spy.DisposeCount.Should().BeGreaterOrEqualTo(1);
        }
    }
}