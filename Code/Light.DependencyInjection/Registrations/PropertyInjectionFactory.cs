using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class PropertyInjectionFactory : InstanceManipulationFactory
    {
        public readonly PropertyInfo PropertyInfo;

        public PropertyInjectionFactory(PropertyInfo propertyInfo) :
            base(propertyInfo.MustNotBeNull(nameof(propertyInfo)).DeclaringType, propertyInfo.Name, propertyInfo.ExtractDependency())
        {
            PropertyInfo = propertyInfo;
        }

        public DependencyFactory DependencyFactory => DependencyFactories[0];

        public override InstanceManipulation Create(string registrationName = "")
        {
            return new PropertyInjection(new TypeKey(TargetType, registrationName), PropertyInfo, DependencyFactories.CreateDependencies());
        }
    }
}