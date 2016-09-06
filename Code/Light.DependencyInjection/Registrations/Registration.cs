using System;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public abstract class Registration : IEquatable<Registration>
    {
        private readonly string _registrationDescription;
        public readonly TypeInfo TargetTypeInfo;
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly TypeKey TypeKey;

        protected Registration(TypeKey typeKey, bool isContainerTrackingDisposable, TypeCreationInfo typeCreationInfo = null)
        {
            TypeKey = typeKey;
            TypeCreationInfo = typeCreationInfo;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
            var registrationNameText = typeKey.RegistrationName == null ? "" : $" \"{typeKey.RegistrationName}\"";
            IsContainerTrackingDisposable = isContainerTrackingDisposable;
            _registrationDescription = $"{GetType().Name}{registrationNameText} for type \"{typeKey.Type}\"";
        }

        public bool IsDefaultRegistration => TypeKey.RegistrationName == null;
        public Type TargetType => TypeKey.Type;
        public string Name => TypeKey.RegistrationName;

        public abstract bool IsCreatingNewInstanceOnNextResolve { get; }

        public bool IsContainerTrackingDisposable { get; }

        public bool Equals(Registration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        public object GetInstance(DiContainer container)
        {
            container.MustNotBeNull(nameof(container));
            CheckIfTargetTypeIsGenericTypeDefinition();

            return GetInstanceInternal(container);
        }

        public object CreateInstance(DiContainer container, ParameterOverrides parameterOverrides)
        {
            container.MustNotBeNull(nameof(container));
            CheckIfTargetTypeIsGenericTypeDefinition();
            CheckIfRegistrationCanCreateInstances();

            return CreateInstanceInternal(container, parameterOverrides);
        }

        protected abstract object CreateInstanceInternal(DiContainer container, ParameterOverrides parameterOverrides);


        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfTargetTypeIsGenericTypeDefinition()
        {
            if (TargetTypeInfo.IsGenericTypeDefinition)
                throw new ResolveTypeException($"The type \"{TargetType}\" is a generic type definition and cannot be resolved.", TargetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfRegistrationCanCreateInstances()
        {
            if (IsCreatingNewInstanceOnNextResolve)
                return;

            throw new ResolveTypeException($"The type {TypeKey.GetFullRegistrationName()} will not be instantiated on the next resolve call, but use an existing instance. Thus it's parameters cannot be overridden.", TargetType);
        }

        protected abstract object GetInstanceInternal(DiContainer container);

        public Registration BindGenericTypeDefinition(Type closedConstructedGenericType)
        {
            var typeInfo = closedConstructedGenericType.GetTypeInfo();
            CheckClosedConstructedType(closedConstructedGenericType, typeInfo);

            return BindGenericTypeDefinition(closedConstructedGenericType, typeInfo);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckClosedConstructedType(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            if (TargetTypeInfo.IsGenericTypeDefinition == false)
                throw new ResolveTypeException($"The type \"{TargetType}\" is no generic type definition and thus no registration can be created for a closed constructed variant of it.", closedConstructedGenericType);

            if (closedConstructedGenericTypeInfo.IsGenericType == false || closedConstructedGenericType.GetGenericTypeDefinition() != TargetType)
                throw new ResolveTypeException($"The type \"{closedConstructedGenericType}\" is not a closed constructed variant of the generic type definition \"{TargetType}\".", closedConstructedGenericType);
        }

        protected abstract Registration BindGenericTypeDefinition(Type closedConstructedGenericType, TypeInfo boundGenericTypeInfo);

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
            return first.EqualsWithHashCode(second);
        }

        public static bool operator !=(Registration first, Registration second)
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return _registrationDescription;
        }
    }
}