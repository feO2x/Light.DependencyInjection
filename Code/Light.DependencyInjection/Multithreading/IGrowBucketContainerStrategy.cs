using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.Multithreading
{
    public interface IGrowBucketContainerStrategy<TRegistration>
    {
        int GetNumberOfBuckets(IReadOnlyList<ImmutableAvlNode<TRegistration>> existingBuckets);
    }
}