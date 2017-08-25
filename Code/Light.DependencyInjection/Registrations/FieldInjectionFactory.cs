using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class FieldInjectionFactory : InstanceManipulationFactory
    {
        public readonly FieldInfo FieldInfo;

        public FieldInjectionFactory(Type targetType, FieldInfo fieldInfo)
            : base(targetType, fieldInfo.MustNotBeNull(nameof(fieldInfo)).Name, fieldInfo.ExtractDependency())
        {
            FieldInfo = fieldInfo;
        }

        public DependencyFactory DependencyFactory => DependencyFactories[0];

        public override InstanceManipulation Create(string registrationName = "")
        {
            return new FieldInjection(new TypeKey(TargetType, registrationName), FieldInfo, DependencyFactories.CreateDependencies());
        }
    }
}