using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a factory that creates instances of <see cref="IConcurrentList{Registration}" />.
    /// </summary>
    public interface IRegistrationCollectionFactory
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IConcurrentList{Registration}" />.
        /// </summary>
        IConcurrentList<Registration> Create();

        /// <summary>
        ///     Creates a new instance of <see cref="IConcurrentList{T}" />, containing the specified items.
        /// </summary>
        /// <param name="items">The items that should be added to the new concurrent list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
        IConcurrentList<Registration> Create(IEnumerable<Registration> items);
    }
}