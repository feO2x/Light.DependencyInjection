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

        public Registration(TypeKey typeKey, Lifetime lifetime, bool shouldTrackDisposables = true)
            : this(typeKey, shouldTrackDisposables)
        {
            CheckLifetime(lifetime);

            Lifetime = lifetime;
        }

        public Registration(Lifetime lifetime, TypeCreationInfo typeCreationInfo, bool shouldTrackDisposables = true)
            : this(typeCreationInfo.TypeKey, shouldTrackDisposables)

        {
            lifetime.MustNotBeNull(nameof(lifetime));

            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
        }

        private Registration(TypeKey typeKey, bool shouldTrackDisposables)
        {
            TypeKey = typeKey;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
            IsTrackingDisposables = TargetTypeInfo.IsImplementingIDisposable() && shouldTrackDisposables;
            _toStringText = TypeKey.GetFullRegistrationName();
        }

        private Registration(TypeKey typeKey, Lifetime lifetime, TypeCreationInfo typeCreationInfo, bool shouldTrackDisposables)
            : this(typeKey, shouldTrackDisposables)
        {
            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
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

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckLifetime(Lifetime lifetime)
        {
            lifetime.MustNotBeNull(nameof(lifetime));
            if (lifetime.RequiresTypeCreationInfo)
                throw new ArgumentException($"You cannot call this constructor with the {lifetime} because it requires a TypeCreationInfo. Use the other constructor instead.");
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