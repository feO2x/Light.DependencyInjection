using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class InstantiationDependency
    {
        public readonly ParameterInfo ParameterInfo;
        public readonly string TargetRegistrationName;
        public readonly TypeKey TypeKey;

        public InstantiationDependency(TypeKey typeKey, ParameterInfo parameterInfo, string targetRegistrationName = null)
        {
            typeKey.MustNotBeEmpty();
            parameterInfo.MustNotBeNull();

            TypeKey = typeKey;
            ParameterInfo = parameterInfo;
            TargetRegistrationName = targetRegistrationName;
        }
    }
}