using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstanceInjection : ISetChildValueRegistrationName
    {
        public readonly string MemberName;
        public readonly Type MemberType;

        protected InstanceInjection(string memberName, Type memberType, string childValueRegistrationName)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));

            MemberName = memberName;
            MemberType = memberType;
            ChildValueRegistrationName = childValueRegistrationName;
        }

        public abstract void InjectValue(object instance, object value);

        public string ChildValueRegistrationName { get; set; }
    }
}