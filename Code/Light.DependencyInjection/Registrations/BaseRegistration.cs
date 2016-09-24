using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class BaseRegistration : IEquatable<BaseRegistration>
    {
        private readonly string _toStringText;
        public readonly bool IsTrackingDisposables;
        public readonly ILifetime Lifetime;
        public readonly TypeInfo TargetTypeInfo;
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly TypeKey TypeKey;

        protected BaseRegistration(TypeKey typeKey,
                                   ILifetime lifetime,
                                   TypeCreationInfo typeCreationInfo = null,
                                   bool isTrackingDisposables = true)
        {
            lifetime.MustNotBeNull(nameof(lifetime));

            TypeKey = typeKey;
            TargetTypeInfo = TypeKey.Type.GetTypeInfo();
            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
            IsTrackingDisposables = TargetTypeInfo.IsImplementingIDisposable() && isTrackingDisposables;
            _toStringText = TypeKey.GetFullRegistrationName();
        }

        public Type TargetType => TypeKey.Type;
        public string Name => TypeKey.RegistrationName;
        public bool IsDefaultRegistration => TypeKey.RegistrationName == null;

        public bool Equals(BaseRegistration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseRegistration);
        }

        public override int GetHashCode()
        {
            return TypeKey.HashCode;
        }

        public static bool operator ==(BaseRegistration first, BaseRegistration second)
        {
            return first.EqualsWithHashCode(second);
        }

        public static bool operator !=(BaseRegistration first, BaseRegistration second)
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return _toStringText;
        }
    }
}