using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DependencyOverrides : IEquatable<DependencyOverrides>
    {
        private readonly Dictionary<Dependency, object> _dependencyOverrides;
        private readonly int _hashCode;

        public DependencyOverrides(Dictionary<Dependency, object> dependencyOverrides)
        {
            _dependencyOverrides = dependencyOverrides.MustNotBeNull(nameof(dependencyOverrides));
            _hashCode = dependencyOverrides.IsNullOrEmpty() ? 0 : Equality.CreateHashCode<Dependency>(_dependencyOverrides.Keys);
        }

        public bool Equals(DependencyOverrides other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;

            if (_dependencyOverrides.Count != other._dependencyOverrides.Count)
                return false;

            foreach (var key in _dependencyOverrides.Keys)
            {
                if (other._dependencyOverrides.ContainsKey(key) == false)
                    return false;
            }

            return true;
        }

        public bool HasDependency(Dependency dependency) => _dependencyOverrides?.ContainsKey(dependency) ?? false;

        public object GetDependencyInstance(Dependency dependency)
        {
            if (_dependencyOverrides.TryGetValue(dependency, out var instance))
                return instance;

            throw new ArgumentException($"The dependency \"{dependency}\" is not overridden.", nameof(dependency));
        }

        public override bool Equals(object obj) => Equals(obj as DependencyOverrides);
        public override int GetHashCode() => _hashCode;
        public static bool operator ==(DependencyOverrides x, DependencyOverrides y) => ReferenceEquals(x, y) || x?._hashCode == y?._hashCode && x.Equals(y);
        public static bool operator !=(DependencyOverrides x, DependencyOverrides y) => !(x == y);

        public DependencyOverrides CreateDependencyOverridesKey()
        {
            var keysOnlyDictionary = new Dictionary<Dependency, object>();
            foreach (var keyValuePair in _dependencyOverrides)
            {
                keysOnlyDictionary.Add(keyValuePair.Key, null);
            }
            return new DependencyOverrides(keysOnlyDictionary);
        }
    }
}