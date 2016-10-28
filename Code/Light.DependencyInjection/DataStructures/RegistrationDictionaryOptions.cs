using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public struct RegistrationDictionaryOptions
    {
        private IGrowBucketContainerStrategy _growContainerStrategy;
        public static readonly IGrowBucketContainerStrategy DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy();

        public IGrowBucketContainerStrategy GrowContainerStrategy
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