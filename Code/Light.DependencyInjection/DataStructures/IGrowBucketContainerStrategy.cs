using System.Collections.Generic;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.DataStructures
{
    public interface IGrowBucketContainerStrategy
    {
        int GetNumberOfBuckets(IReadOnlyList<ImmutableAvlNode<Registration>> existingBuckets);
    }
}