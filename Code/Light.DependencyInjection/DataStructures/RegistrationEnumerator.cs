using System;
using System.Collections;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public struct RegistrationEnumerator : IEnumerator<Registration>
    {
        private readonly Type _targetType;
        private Registration _currentRegistration;
        private AvlTreeEnumerator<Registration> _treeEnumerator;

        public RegistrationEnumerator(Type targetType, AvlTreeEnumerator<Registration> treeEnumerator)
        {
            targetType.MustNotBeNull(nameof(targetType));

            _targetType = targetType;
            _treeEnumerator = treeEnumerator;
            _currentRegistration = null;
        }

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

        public void Reset()
        {
            _treeEnumerator.Reset();
            _currentRegistration = null;
        }

        public Registration Current => _currentRegistration;

        object IEnumerator.Current => _currentRegistration;

        public void Dispose() { }

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