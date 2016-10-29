using System;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents the abstraction of deciding when the <see cref="ImmutableRegistrationBuckets" /> instance will increase its number of buckets.
    /// </summary>
    public interface IGrowBucketsStrategy
    {
        /// <summary>
        ///     Gets the number of buckets that a new instance of <see cref="ImmutableRegistrationBuckets" /> should use.
        /// </summary>
        /// <param name="existingBuckets">The existing buckets instance which is currently creating a new one.</param>
        /// <returns>The number of buckets to be used in a new instance of <see cref="ImmutableRegistrationBuckets" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingBuckets" /> is null.</exception>
        int GetNumberOfBuckets(ImmutableRegistrationBuckets existingBuckets);
    }
}