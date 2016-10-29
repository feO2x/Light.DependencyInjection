using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public struct RegistrationDictionaryOptions
    {
        private IGrowBucketsStrategy _growContainerStrategy;
        public static readonly IGrowBucketsStrategy DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy();

        public IGrowBucketsStrategy GrowContainerStrategy
        {
            get { return _growContainerStrategy; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _growContainerStrategy = value;
            }
        }

        public static RegistrationDictionaryOptions Create()
        {
            return new RegistrationDictionaryOptions
                   {
                       _growContainerStrategy = DefaultGrowContainerStrategy
                   };
        }
    }
}