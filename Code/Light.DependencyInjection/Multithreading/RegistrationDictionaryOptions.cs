using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct RegistrationDictionaryOptions<TRegistration>
    {
        private IGrowBucketContainerStrategy<TRegistration> _growContainerStrategy;
        public static readonly IGrowBucketContainerStrategy<TRegistration> DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy<TRegistration>();

        public IGrowBucketContainerStrategy<TRegistration> GrowContainerStrategy
        {
            get { return _growContainerStrategy; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _growContainerStrategy = value;
            }
        }

        public static RegistrationDictionaryOptions<TRegistration> Create()
        {
            return new RegistrationDictionaryOptions<TRegistration>
                   {
                       _growContainerStrategy = DefaultGrowContainerStrategy
                   };
        }
    }
}