using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public struct RegistrationEnumerator<TRegistration> : IEnumerator<TRegistration>
    {
        private readonly Type _targetType;
        private TRegistration _currentRegistration;
        private AvlTreeEnumerator<TRegistration> _treeEnumerator;

        public RegistrationEnumerator(Type targetType, AvlTreeEnumerator<TRegistration> treeEnumerator)
        {
            targetType.MustNotBeNull(nameof(targetType));

            _targetType = targetType;
            _treeEnumerator = treeEnumerator;
            _currentRegistration = default(TRegistration);
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
            _currentRegistration = default(TRegistration);
            return false;
        }

        public void Reset()
        {
            _treeEnumerator.Reset();
            _currentRegistration = default(TRegistration);
        }

        public TRegistration Current => _currentRegistration;

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