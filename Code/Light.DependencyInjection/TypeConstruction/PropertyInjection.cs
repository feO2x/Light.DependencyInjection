using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class PropertyInjection : InstanceInjection
    {
        public readonly PropertyInfo PropertyInfo;

        public PropertyInjection(PropertyInfo propertyInfo, string targetRegistrationName = null)
            : base(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo.DeclaringType, CreateSetValueAction(propertyInfo), targetRegistrationName)
        {
            CheckPropertyInfo(propertyInfo);

            PropertyInfo = propertyInfo;
        }

        private static Action<object, object> CreateSetValueAction(PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var instanceDowncast = Expression.Convert(instanceParameter, propertyInfo.DeclaringType);
            var valueDowncast = Expression.Convert(valueParameter, propertyInfo.PropertyType);

            var callSetter = Expression.Call(instanceDowncast, propertyInfo.SetMethod, valueDowncast);
            return Expression.Lambda<Action<object, object>>(callSetter, instanceParameter, valueParameter).Compile();
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsPublicSettableInstancePropertyInfo())
                return;

            throw new TypeRegistrationException($"The property info \"{propertyInfo}\" does not describe a public instance property with a setter.", propertyInfo.DeclaringType);
        }

        protected override InstanceInjection BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            return new PropertyInjection(closedGenericTypeInfo.GetDeclaredProperty(PropertyInfo.Name), TargetRegistrationName);
        }
    }
}