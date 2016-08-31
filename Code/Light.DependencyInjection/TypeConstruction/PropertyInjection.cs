using System;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class PropertyInjection : InstanceInjection
    {
        public readonly PropertyInfo PropertyInfo;

        public PropertyInjection(PropertyInfo propertyInfo, string childValueRegistrationName = null) 
            : base(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo.DeclaringType, childValueRegistrationName)
        {
            CheckPropertyInfo(propertyInfo);

            PropertyInfo = propertyInfo;
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
            PropertyInfo.SetValue(instance, value);
        }

        protected override InstanceInjection CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            return new PropertyInjection(closedConstructedGenericTypeInfo.GetDeclaredProperty(PropertyInfo.Name), ChildValueRegistrationName);
        }
    }
}