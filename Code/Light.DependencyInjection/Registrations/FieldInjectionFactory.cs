using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class FieldInjectionFactory : InstanceManipulationFactory
    {
        public readonly FieldInfo FieldInfo;

        public FieldInjectionFactory(FieldInfo fieldInfo)
            : base(fieldInfo.MustNotBeNull(nameof(fieldInfo)).DeclaringType, fieldInfo.Name, fieldInfo.ExtractDependency())
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