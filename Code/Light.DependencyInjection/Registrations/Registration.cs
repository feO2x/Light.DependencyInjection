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
            CheckIfTargetTypeIsGenericTypeDefinition();

            return GetInstanceInternal(container);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckIfTargetTypeIsGenericTypeDefinition()
        {
            if (TargetTypeInfo.IsGenericTypeDefinition)
                throw new ResolveTypeException($"The type \"{TargetType}\" is a generic type definition and cannot be resolved.", TargetType);
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
    }
}