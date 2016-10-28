using System;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents a registration that encapsulates instantiation and lifetime information about a type.
    /// </summary>
    public sealed class Registration : IEquatable<Registration>
    {
        /// <summary>
        ///     Gets or sets the value indicating whether <see cref="ToString" /> will return string representations with the Type.FullName
        ///     or Type.Name. Defaults to false. This value should be set before the first registration is created.
        /// </summary>
        public static bool UseFullTypeNamesForToString = false;

        private readonly string _toStringText;

        /// <summary>
        ///     Gets the value indicating whether the DI container tracks disposable instances (only applies to types that implement <see cref="IDisposable" />).
        /// </summary>
        public readonly bool IsTrackingDisposables;

        /// <summary>
        ///     Gets the lifetime that is associated with this registration.
        /// </summary>
        public readonly Lifetime Lifetime;

        /// <summary>
        ///     Gets the type info for the target type.
        /// </summary>
        public readonly TypeInfo TargetTypeInfo;

        /// <summary>
        ///     Gets the TypeCreationInfo that is used to instantiate the target type. This might be null if the type is not instantiated by the DI container (e.g. for a <see cref="ExternalInstanceLifetime" />).
        /// </summary>
        public readonly TypeCreationInfo TypeCreationInfo;

        /// <summary>
        ///     Gets the type key the uniquely identifies this Registration.
        /// </summary>
        public readonly TypeKey TypeKey;

        /// <summary>
        ///     Creates a new instance of <see cref="Registration" /> with a lifetime that does not require a <see cref="TypeCreationInfo" /> (e.g. the <see cref="ExternalInstanceLifetime" />).
        /// </summary>
        /// <param name="typeKey">The type key that uniquely identifies this registration.</param>
        /// <param name="lifetime">The lifetime of the registration. This instance must return false for Lifetime.RequiresTypeCreationInfo.</param>
        /// <param name="shouldTrackDisposables">The value indicating whether the DI container should track disposable instances (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lifetime" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="lifetime" />.RequiresTypeCreationInfo returns true.</exception>
        public Registration(TypeKey typeKey, Lifetime lifetime, bool shouldTrackDisposables = true)
        {
            CheckLifetime(lifetime);

            TypeKey = typeKey;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
            Lifetime = lifetime;
            IsTrackingDisposables = CheckIfDisposableInstancesShouldBeTracked(shouldTrackDisposables);
            _toStringText = CreateToStringText();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Registration" /> with a lifetime and type creation info.
        /// </summary>
        /// <param name="lifetime">The lifetime of the registration.</param>
        /// <param name="typeCreationInfo">The type creation info used to instantiate the target type.</param>
        /// <param name="shouldTrackDisposables">The value indicating whether the DI container should track disposable instances (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lifetime" /> or <paramref name="typeCreationInfo" /> is null.</exception>
        public Registration(Lifetime lifetime, TypeCreationInfo typeCreationInfo, bool shouldTrackDisposables = true)
        {
            lifetime.MustNotBeNull(nameof(lifetime));
            typeCreationInfo.MustNotBeNull(nameof(typeCreationInfo));

            TypeKey = typeCreationInfo.TypeKey;
            TargetTypeInfo = typeCreationInfo.TargetTypeInfo;
            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
            IsTrackingDisposables = CheckIfDisposableInstancesShouldBeTracked(shouldTrackDisposables);
            _toStringText = CreateToStringText();
        }

        private Registration(TypeKey typeKey, Lifetime lifetime, TypeCreationInfo typeCreationInfo, bool isTrackingDisposables)
        {
            TypeKey = typeKey;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
            Lifetime = lifetime;
            TypeCreationInfo = typeCreationInfo;
            IsTrackingDisposables = isTrackingDisposables;
            _toStringText = CreateToStringText();
        }

        /// <summary>
        ///     Gets the target type of this registration.
        /// </summary>
        public Type TargetType => TypeKey.Type;

        /// <summary>
        ///     Gets the name of this registration.
        /// </summary>
        public string Name => TypeKey.RegistrationName;

        /// <summary>
        ///     Gets the value indicating whether this registration is a default registration. This is true when <see cref="Name" /> is null.
        /// </summary>
        public bool IsDefaultRegistration => TypeKey.RegistrationName == null;

        /// <summary>
        ///     Gets the value indicating whether this registration's target type is a generic type definition.
        ///     This kind of registration cannot be instantiated, but serve as a template when they are bound to
        ///     a closed generic type variant of the generic type definition (<see cref="BindToClosedGenericType" />).
        /// </summary>
        public bool IsRegistrationForGenericTypeDefinition => TargetTypeInfo.IsGenericTypeDefinition;

        /// <summary>
        ///     Checks if the other registration is equal to this instance.
        /// </summary>
        /// <returns>True if the other registration has the same type key, else false.</returns>
        public bool Equals(Registration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        private bool CheckIfDisposableInstancesShouldBeTracked(bool shouldTrackDisposables)
        {
            return TargetTypeInfo.IsImplementingIDisposable() && shouldTrackDisposables;
        }

        private string CreateToStringText()
        {
            var typeName = UseFullTypeNamesForToString ? TargetType.FullName : TargetType.Name;
            return $"Registration for type \"{typeName}\"{TypeKey.GetWithRegistrationNameText()}";
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckLifetime(Lifetime lifetime)
        {
            lifetime.MustNotBeNull(nameof(lifetime));
            if (lifetime.RequiresTypeCreationInfo)
                throw new ArgumentException($"You cannot call this constructor with the {lifetime} because it requires a TypeCreationInfo. Use the other constructor instead.");
        }

        /// <summary>
        ///     Creates a new registration for the <paramref name="closedGenericType" /> with all the settings copied from this
        ///     registration of a generic type definition.
        /// </summary>
        /// <param name="closedGenericType">A closed generic variant of the generic type definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="closedGenericType" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when this registration's target type is not a generic type definition (i.e. <see cref="IsRegistrationForGenericTypeDefinition" /> returns false).</exception>
        /// <exception cref="ResolveTypeException">Thrown when <paramref name="closedGenericType" />is not a closed generic type variant of the target type.</exception>
        public Registration BindToClosedGenericType(Type closedGenericType)
        {
            closedGenericType.MustBeClosedVariantOf(TargetType);

            return new Registration(new TypeKey(closedGenericType, Name),
                                    Lifetime.BindToClosedGenericType(),
                                    TypeCreationInfo?.BindToClosedGenericType(closedGenericType, closedGenericType.GetTypeInfo()),
                                    IsTrackingDisposables);
        }

        /// <summary>
        ///     Checks if the specified object is equal to this instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as Registration);
        }

        /// <summary>
        ///     Returns the hash code of the <see cref="TypeKey" />.
        /// </summary>
        public override int GetHashCode()
        {
            return TypeKey.HashCode;
        }

        /// <summary>
        ///     Checks if the specified registration instances are equal.
        /// </summary>
        public static bool operator ==(Registration first, Registration second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            if (ReferenceEquals(second, null))
                return false;

            return first.TypeKey == second.TypeKey;
        }

        /// <summary>
        ///     Checks if the specified registration instances are not equal.
        /// </summary>
        public static bool operator !=(Registration first, Registration second)
        {
            return !(first == second);
        }

        /// <summary>
        ///     Returns the string representation of this registration. <see cref="UseFullTypeNamesForToString" /> can be used
        ///     to configure if the Type.FullName or Type.Name should be used.
        /// </summary>
        public override string ToString()
        {
            return _toStringText;
        }
    }
}