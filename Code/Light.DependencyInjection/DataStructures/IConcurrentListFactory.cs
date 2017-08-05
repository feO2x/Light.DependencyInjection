using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a factory that creates instances of <see cref="IConcurrentList{T}" />.
    /// </summary>
    public interface IConcurrentListFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IConcurrentList{T}" />.
        /// </summary>
        /// <typeparam name="T">The item type of the list.</typeparam>
        IConcurrentList<T> Create<T>();

        /// <summary>
        ///     Creates a new instance of <see cref="IConcurrentList{T}" />, containing the specified items.
        /// </summary>
        /// <typeparam name="T">The item type of the list.</typeparam>
        /// <param name="items">The items that should be added to the new concurrent list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
        IConcurrentList<T> Create<T>(IEnumerable<T> items);
    }
}