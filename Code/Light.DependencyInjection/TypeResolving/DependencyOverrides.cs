using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DependencyOverrides : IEquatable<DependencyOverrides>
    {
        private readonly Dictionary<Dependency, object> _overriddenDependencies;
        private readonly Dictionary<TypeKey, object> _overriddenRegistrations;
        private readonly int _hashCode;

        public DependencyOverrides(Dictionary<Dependency, object> overriddenDependencies, Dictionary<TypeKey, object> overriddenRegistrations)
        {
            _overriddenDependencies = overriddenDependencies;
            _overriddenRegistrations = overriddenRegistrations;

            _hashCode = Equality.CreateHashCode(overriddenDependencies.IsNullOrEmpty() ? 0 : Equality.CreateHashCode<Dependency>(overriddenDependencies.Keys),
                                                overriddenRegistrations.IsNullOrEmpty() ? 0 : Equality.CreateHashCode<TypeKey>(overriddenRegistrations.Keys));
        }

        public bool Equals(DependencyOverrides other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;

            if (_overriddenDependencies.Count != other._overriddenDependencies.Count)
                return false;

            foreach (var key in _overriddenDependencies.Keys)
            {
                if (other._overriddenDependencies.ContainsKey(key) == false)
                    return false;
            }

            return true;
        }

        public bool HasDependency(Dependency dependency) => _overriddenDependencies?.ContainsKey(dependency) ?? false;

        public object GetDependencyInstance(Dependency dependency)
        {
            if (_overriddenDependencies.TryGetValue(dependency, out var instance))
                return instance;

            throw new ArgumentException($"The dependency \"{dependency}\" is not overridden.", nameof(dependency));
        }

        public bool HasOverriddenInstance(TypeKey typeKey) => _overriddenRegistrations?.ContainsKey(typeKey) ?? false;

        public object GetOverriddenInstance(TypeKey typeKey)
        {
            if (_overriddenRegistrations.TryGetValue(typeKey, out var instance))
                return instance;

            throw new ArgumentException($"The registration {typeKey} is not overridden.", nameof(typeKey));
        }

        public override bool Equals(object obj) => Equals(obj as DependencyOverrides);
        public override int GetHashCode() => _hashCode;
        public static bool operator ==(DependencyOverrides x, DependencyOverrides y) => ReferenceEquals(x, y) || x?._hashCode == y?._hashCode && x.Equals(y);
        public static bool operator !=(DependencyOverrides x, DependencyOverrides y) => !(x == y);

        public DependencyOverrides CreateDependencyOverridesKey()
        {
            Dictionary<Dependency, object> keysOnlyDependencyOverrides = null;
            if (_overriddenDependencies.IsNullOrEmpty() == false)
            {
                keysOnlyDependencyOverrides = new Dictionary<Dependency, object>();
                foreach (var keyValuePair in _overriddenDependencies)
                {
                    keysOnlyDependencyOverrides.Add(keyValuePair.Key, null);
                }
            }

            Dictionary<TypeKey, object> keysOnlyOverriddenRegistrations = null;
            if (_overriddenRegistrations.IsNullOrEmpty() == false)
            {
                keysOnlyOverriddenRegistrations = new Dictionary<TypeKey, object>();
                foreach (var keyValuePair in _overriddenRegistrations)
                {
                    keysOnlyOverriddenRegistrations.Add(keyValuePair.Key, null);
                }
            }

            return new DependencyOverrides(keysOnlyDependencyOverrides, keysOnlyOverriddenRegistrations);
        }
    }
}