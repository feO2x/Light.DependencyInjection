using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class PropertyInjectionFactory : InstanceManipulationFactory
    {
        public readonly PropertyInfo PropertyInfo;

        public PropertyInjectionFactory(Type targetType, PropertyInfo propertyInfo) :
            base(targetType, propertyInfo.MustNotBeNull(nameof(propertyInfo)).Name, propertyInfo.ExtractDependency())
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