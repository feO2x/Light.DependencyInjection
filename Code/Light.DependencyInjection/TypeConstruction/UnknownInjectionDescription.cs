using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct UnknownInjectionDescription
    {
        public readonly MemberInfo MemberInfo;
        public readonly object Value;

        public UnknownInjectionDescription(MemberInfo memberInfo, object value)
        {
            memberInfo.MustNotBeNull(nameof(memberInfo));

            MemberInfo = memberInfo;
            Value = value;
        }
    }
}