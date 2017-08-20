using System.Collections.Generic;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.DataStructures
{
    public sealed class ReaderWriterLockedListFactory : IRegistrationCollectionFactory
    {
        public IConcurrentList<Registration> Create()
        {
            return new ReaderWriterLockedList<Registration>(equalityComparer: RegistrationNameComparer.Instance);
        }

        public IConcurrentList<Registration> Create(IEnumerable<Registration> existingRegistrations)
        {
            return new ReaderWriterLockedList<Registration>(existingRegistrations, RegistrationNameComparer.Instance);
        }
    }
}