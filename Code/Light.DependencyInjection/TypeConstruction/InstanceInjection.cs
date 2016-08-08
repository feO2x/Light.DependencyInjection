using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstanceInjection
    {
        public readonly string MemberName;
        public readonly Type MemberType;
        protected InstanceInjection(string memberName, Type memberType)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));

            MemberName = memberName;
            MemberType = memberType;
        }

        public abstract void InjectValue(object instance, object value);
    }
}