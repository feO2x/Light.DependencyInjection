namespace Light.DependencyInjection.DataStructures
{
    public sealed class DefaultConcurrentDictionaryFactory : IConcurrentDictionaryFactory
    {
        public IConcurrentDictionary<TKey, TValue> Create<TKey, TValue>()
        {
            return new ConcurrentDictionary<TKey, TValue>();
        }
    }
}