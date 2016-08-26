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

        protected InstanceInjection(string memberName, Type memberType, Type declaringType, string childValueRegistrationName)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));
            declaringType.MustNotBeNull(nameof(declaringType));

            MemberName = memberName;
            MemberType = memberType;
            DeclaringType = declaringType;
            ChildValueRegistrationName = childValueRegistrationName;
        }

        string ISetChildValueRegistrationName.ChildValueRegistrationName
        {
            set { ChildValueRegistrationName = value; }
        }

        public abstract void InjectValue(object instance, object value);

        public InstanceInjection CloneForBoundGenericType(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            boundGenericType.MustBeBoundVersionOfUnboundGenericType(DeclaringType);

            return CloneForBoundGenericTypeInternal(boundGenericType, boundGenericTypeInfo);
        }

        protected abstract InstanceInjection CloneForBoundGenericTypeInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo);
    }
}