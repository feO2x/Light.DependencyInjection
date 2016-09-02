using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstanceInjection : ISetChildValueRegistrationName
    {
        public readonly string MemberName;
        public readonly Type MemberType;
        public readonly Type DeclaringType;
        public string ChildValueRegistrationName;
        public readonly string DisplayName;

        protected InstanceInjection(string memberName, Type memberType, Type declaringType, string childValueRegistrationName)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));
            declaringType.MustNotBeNull(nameof(declaringType));

            MemberName = memberName;
            MemberType = memberType;
            DeclaringType = declaringType;
            ChildValueRegistrationName = childValueRegistrationName;
            DisplayName = $"{GetType().Name} {DeclaringType.Name}.{MemberName}";
        }

        string ISetChildValueRegistrationName.ChildValueRegistrationName
        {
            set { ChildValueRegistrationName = value; }
        }

        public abstract void InjectValue(object instance, object value);

        public InstanceInjection CloneForClosedConstructedGenericType(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            closedConstructedGenericType.MustBeClosedConstructedVariantOf(DeclaringType);

            return CloneForClosedConstructedGenericTypeInternal(closedConstructedGenericType, closedConstructedGenericTypeInfo);
        }

        protected abstract InstanceInjection CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo);

        public override string ToString()
        {
            return DisplayName;
        }
    }
}