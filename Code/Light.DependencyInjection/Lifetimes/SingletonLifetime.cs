using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a singleton lifetime that lasts the whole process lifetime.
    /// </summary>
    public sealed class SingletonLifetime : Lifetime
    {
        private object _instance;

        /// <summary>
        ///     Creates an instance if it has not been created before, otherwise returns the existing singleton.
        /// </summary>
        public override object GetInstance(CreationContext context)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = context.CreateInstance();
                }
            }
            return _instance;
        }

        /// <summary>
        ///     Returns a new instance of this lifetime.
        /// </summary>
        public override Lifetime BindToClosedGenericType()
        {
            return new SingletonLifetime();
        }
    }
}