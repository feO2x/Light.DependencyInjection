using System;

namespace Light.DependencyInjection.Threading
{
    public interface IReaderWriterLock : IDisposable
    {
        void EnterReadLock();
        void ExitReadLock();
        void EnterUpgradeableReadLock();
        void ExitUpgradeableReadLock();
        void EnterWriteLock();
        void ExitWriteLock();
    }
}