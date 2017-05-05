using System;

namespace Light.DependencyInjection.Threading
{
    public interface IReaderWriterLock : IDisposable
    {
        void EnterReadLock();
        void ExitReadLock();
        void EnterWriteLock();
        void ExitWriteLock();
    }
}