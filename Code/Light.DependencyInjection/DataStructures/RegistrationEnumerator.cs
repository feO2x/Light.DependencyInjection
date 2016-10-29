using System;
using System.Collections;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents an <see cref="IEnumerator{T}" /> that can iterate over all registrations whose mapping keys belong too the specified target type.
    /// </summary>
    public struct RegistrationEnumerator : IEnumerator<Registration>
    {
        private readonly Type _targetType;
        private Registration _currentRegistration;
        private AvlTreeEnumerator<Registration> _treeEnumerator;

        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationEnumerator" />.
        /// </summary>
        /// <param name="targetType">The target type to be filtered.</param>
        /// <param name="treeEnumerator">The enumerator that iterates over a single AVL tree.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> is null.</exception>
        public RegistrationEnumerator(Type targetType, AvlTreeEnumerator<Registration> treeEnumerator)
        {
            targetType.MustNotBeNull(nameof(targetType));

            _targetType = targetType;
            _treeEnumerator = treeEnumerator;
            _currentRegistration = null;
        }

        /// <summary>
        ///     Advances the enumerator to the next registration.
        /// </summary>
        /// <returns>True if the enumerator was successfully advanced to the next registration; false if the enumerator has passed the last registration.</returns>
        public bool MoveNext()
        {
            while (_treeEnumerator.MoveNext())
            {
                if (_treeEnumerator.Current.HashEntry.Key.Type != _targetType)
                    continue;

                _currentRegistration = _treeEnumerator.Current.HashEntry.Value;
                return true;
            }
            _currentRegistration = null;
            return false;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first registration.
        /// </summary>
        public void Reset()
        {
            _treeEnumerator.Reset();
            _currentRegistration = null;
        }

        /// <summary>
        ///     Gets the current registration.
        /// </summary>
        public Registration Current => _currentRegistration;

        object IEnumerator.Current => _currentRegistration;

        /// <summary>
        ///     Does nothing because this enumerator references no resources that must be diposed.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///     Gets the number of registrations that this enumerator will iterate over.
        ///     The enumerator is reset during this operation.
        /// </summary>
        public int GetNumberOfRegistrations()
        {
            Reset();
            var numberOfRegistrations = 0;
            while (MoveNext())
                ++numberOfRegistrations;
            Reset();
            return numberOfRegistrations;
        }
    }
}