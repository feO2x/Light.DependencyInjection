namespace Light.DependencyInjection.DataStructures
{
    public interface IConcurrentDictionaryFactory
    {
        IConcurrentDictionary<TKey, TValue> Create<TKey, TValue>();
    }
}