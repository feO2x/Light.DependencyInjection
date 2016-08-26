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
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly TypeKey TypeKey;
        public readonly TypeInfo TargetTypeInfo;

        protected Registration(TypeKey typeKey, TypeCreationInfo typeCreationInfo = null)
        {
            TypeKey = typeKey;
            TypeCreationInfo = typeCreationInfo;
            TargetTypeInfo = typeKey.Type.GetTypeInfo();
        }

        public bool IsDefaultRegistration => TypeKey.RegistrationName == null;
        public Type TargetType => TypeKey.Type;
        public string Name => TypeKey.RegistrationName;

        public bool Equals(Registration other)
        {
            if (other == null)
                return false;

            return TypeKey == other.TypeKey;
        }

        public object GetInstance(DiContainer container)
        {
            container.MustNotBeNull(nameof(container));
            CheckIfTargetTypeIsNotUnbound();

            return GetInstanceInternal(container);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfTargetTypeIsNotUnbound()
        {
            if (TargetTypeInfo.IsGenericTypeDefinition)
                throw new ResolveTypeException($"The type \"{TargetType}\" is an unbound generic type and cannot be resolved.", TargetType);
        }

        protected abstract object GetInstanceInternal(DiContainer container);

        public Registration BindGenericTypeRegistration(Type boundGenericType)
        {
            var typeInfo = boundGenericType.GetTypeInfo();
            CheckBoundGenericType(boundGenericType, typeInfo);

            return BindGenericTypeRegistrationInternal(boundGenericType, typeInfo);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckBoundGenericType(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            if (TargetTypeInfo.IsGenericTypeDefinition == false)
                throw new ResolveTypeException($"The type \"{TargetType}\" is no unbound generic type and thus no registration can be created for a resolved generic version of it.", boundGenericType);

            if (boundGenericTypeInfo.IsGenericType == false || boundGenericType.GetGenericTypeDefinition() != TargetType)
                throw new ResolveTypeException($"The type \"{boundGenericType}\" is not a resolved version of the unbound generic type \"{TargetType}\".", boundGenericType);
        }

        protected abstract Registration BindGenericTypeRegistrationInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo);

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
    }
}