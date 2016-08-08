using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class PropertyInjection : InstanceInjection
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyInjection(PropertyInfo propertyInfo) : base(propertyInfo.Name, propertyInfo.PropertyType)
        {
            CheckPropertyInfo(propertyInfo);

            _propertyInfo = propertyInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo.SetMethod != null &&
                propertyInfo.SetMethod.IsPublic &&
                propertyInfo.SetMethod.IsStatic == false)
                return;

            throw new TypeRegistrationException($"The property info \"{propertyInfo}\" does not describe a public instance property with a setter.", propertyInfo.DeclaringType);
        }

        public override void InjectValue(object instance, object value)
        {
            _propertyInfo.SetValue(instance, value);
        }
    }
}