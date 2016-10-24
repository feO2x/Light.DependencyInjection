using System;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration : IEquatable<Registration>
    {
        private readonly string _toStringText;
        public readonly bool IsTrackingDisposables;
        public readonly Lifetime Lifetime;
        public readonly TypeInfo TargetTypeInfo;
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly TypeKey TypeKey;

        public Registration(TypeKey typeKey,
                            Lifetime lifetime,
                            TypeCreationInfo typeCreationInfo = null,
                            bool shouldTrackDisposables = true)

        {
            lifetime.MustNotBeNull(nameof(lifetime));

            TypeKey = typeKey;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
            IsTrackingDisposables = TargetTypeInfo.IsImplementingIDisposable() && shouldTrackDisposables;
            _toStringText = TypeKey.GetFullRegistrationName();
        }

        public Type TargetType => TypeKey.Type;
        public string Name => TypeKey.RegistrationName;
        public bool IsDefaultRegistration => TypeKey.RegistrationName == null;
        public bool IsRegistrationForGenericTypeDefinition => TargetTypeInfo.IsGenericTypeDefinition;

        public bool Equals(Registration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        public Registration BindToClosedGenericType(Type closedGenericType)
        {
            closedGenericType.MustBeClosedVariantOf(TargetType);

            return new Registration(new TypeKey(closedGenericType, Name),
                                    Lifetime.ProvideInstanceForResolvedGenericTypeDefinition(),
                                    TypeCreationInfo?.BindToClosedGenericType(closedGenericType, closedGenericType.GetTypeInfo()),
                                    IsTrackingDisposables);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Registration);
        }

        public override int GetHashCode()
        {
            return TypeKey.HashCode;
        }

        public static bool operator ==(Registration first, Registration second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            if (ReferenceEquals(second, null))
                return false;

            return first.TypeKey == second.TypeKey;
        }

        public static bool operator !=(Registration first, Registration second)
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return _toStringText;
        }
    }
}