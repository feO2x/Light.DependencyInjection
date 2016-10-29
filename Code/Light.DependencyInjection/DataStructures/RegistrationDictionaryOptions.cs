using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents the options that configure the <see cref="IGrowBucketsStrategy" /> for the <see cref="RegistrationDictionary" />.
    /// </summary>
    public struct RegistrationDictionaryOptions
    {
        private IGrowBucketsStrategy _growContainerStrategy;

        /// <summary>
        ///     Gets the default grow buckets strategy which is an instance of <see cref="PrimeNumberLinearStrategy" />.
        /// </summary>
        public static readonly IGrowBucketsStrategy DefaultGrowBucketsStrategy = new PrimeNumberLinearStrategy();

        /// <summary>
        ///     Gets or sets the <see cref="IGrowBucketsStrategy" /> that is used internally to decide when to grow the number of buckets of the dictionary.
        /// </summary>
        public IGrowBucketsStrategy GrowContainerStrategy
        {
            get { return _growContainerStrategy; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _growContainerStrategy = value;
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="RegistrationDictionaryOptions" /> with the default values.
        /// </summary>
        public static RegistrationDictionaryOptions Create()
        {
            return new RegistrationDictionaryOptions
                   {
                       _growContainerStrategy = DefaultGrowBucketsStrategy
                   };
        }
    }
}