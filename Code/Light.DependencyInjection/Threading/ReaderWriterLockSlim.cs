using System.Threading;

namespace Light.DependencyInjection.Threading
{
    public sealed class ReaderWriterLockSlim : System.Threading.ReaderWriterLockSlim, IReaderWriterLock
    {
        public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) : base(recursionPolicy) { }
    }
}