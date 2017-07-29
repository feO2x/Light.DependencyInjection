using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class InstantionDependencyFactory
    {
        public readonly ParameterInfo ParameterInfo;
        public readonly Type TargetType;
        private string _targetRegistrationName = "";

        public InstantionDependencyFactory(Type targetType, ParameterInfo parameterInfo)
        {
            TargetType = targetType.MustNotBeNull();
            ParameterInfo = parameterInfo.MustNotBeNull();
        }

        public string TargetRegistrationName
        {
            get => _targetRegistrationName;
            set => _targetRegistrationName = value.MustNotBeNull();
        }

        public InstantiationDependency Create(string registrationName)
        {
            return new InstantiationDependency(new TypeKey(TargetType, registrationName),
                                               ParameterInfo,
                                               TargetRegistrationName);
        }
    }
}