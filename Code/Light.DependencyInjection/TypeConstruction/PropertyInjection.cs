using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class PropertyInjection : InstanceInjection
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyInjection(PropertyInfo propertyInfo, string childValueRegistrationName = null) : base(propertyInfo.Name, propertyInfo.PropertyType, childValueRegistrationName)
        {
            CheckPropertyInfo(propertyInfo);

            _propertyInfo = propertyInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsPublicSettableInstancePropertyInfo())
                return;

            throw new TypeRegistrationException($"The property info \"{propertyInfo}\" does not describe a public instance property with a setter.", propertyInfo.DeclaringType);
        }

        public override void InjectValue(object instance, object value)
        {
            _propertyInfo.SetValue(instance, value);
        }
    }
}