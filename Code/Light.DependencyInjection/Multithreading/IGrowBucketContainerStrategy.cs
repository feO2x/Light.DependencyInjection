using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.Multithreading
{
    public interface IGrowBucketContainerStrategy<TKey, TValue> where TKey : IEquatable<TKey>
    {
        int GetNumberOfBuckets(IReadOnlyList<ImmutableAvlNode<TKey, TValue>> existingBuckets);
    }
}