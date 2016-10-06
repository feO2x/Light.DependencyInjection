using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct RegistrationDictionaryOptions<TRegistration>
    {
        private IGrowBucketContainerStrategy<TypeKey, TRegistration> _growContainerStrategy;
        public static readonly IGrowBucketContainerStrategy<TypeKey, TRegistration> DefaultGrowContainerStrategy = new PrimeNumberLinearStrategy<TypeKey, TRegistration>();

        public IGrowBucketContainerStrategy<TypeKey, TRegistration> GrowContainerStrategy
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