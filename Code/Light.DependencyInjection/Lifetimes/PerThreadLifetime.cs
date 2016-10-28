using System.Threading;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that creates instances once per thread.
    /// </summary>
    public sealed class PerThreadLifetime : Lifetime
    {
        private readonly ThreadLocal<object> _threadLocal = new ThreadLocal<object>();

        /// <summary>
        ///     Creates a new instance if it does not exist for the current thread, else the existing instance will be returned.
        /// </summary>
        public override object GetInstance(ResolveContext context)
        {
            if (_threadLocal.IsValueCreated == false)
                _threadLocal.Value = context.CreateInstance();

            return _threadLocal.Value;
        }

        /// <summary>
        ///     Returns a new instance of this lifetime.
        /// </summary>
        public override Lifetime BindToClosedGenericType()
        {
            return new PerThreadLifetime();
        }
    }
}