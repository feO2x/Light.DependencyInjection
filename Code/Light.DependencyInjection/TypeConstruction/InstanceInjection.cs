using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstanceInjection : ISetResolvedRegistrationName
    {
        public readonly string MemberName;
        public readonly Type MemberType;

        protected InstanceInjection(string memberName, Type memberType, string resolvedRegistrationName)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));

            MemberName = memberName;
            MemberType = memberType;
            ResolvedRegistrationName = resolvedRegistrationName;
        }

        public abstract void InjectValue(object instance, object value);

        public string ResolvedRegistrationName { get; set; }
    }
}