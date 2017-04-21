using Light.DependencyInjection.DataStructures;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServices
    {
        private IConcurrentDictionaryFactory _concurrentDictionaryFactory = new DefaultConcurrentDictionaryFactory();

        public IConcurrentDictionaryFactory ConcurrentDictionaryFactory
        {
            get => _concurrentDictionaryFactory;
            set
            {
                value.MustNotBeNull();
                _concurrentDictionaryFactory = value;
            }
        }
    }
}