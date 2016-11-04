using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="InstanceInjection" /> that injects a value into a property of the target instance.
    /// </summary>
    public sealed class PropertyInjection : InstanceInjection
    {
        /// <summary>
        ///     Gets the info of the property that is called internally.
        /// </summary>
        public readonly PropertyInfo PropertyInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="PropertyInjection" />.
        /// </summary>
        /// <param name="targetType">The type that the property info belongs to.</param>
        /// <param name="propertyInfo">The info of the property that should be populated.</param>
        /// <param name="targetRegistrationName">The name of the target registration whose resolved value should be injected into the property (optional).</param>
        public PropertyInjection(Type targetType, PropertyInfo propertyInfo, string targetRegistrationName = null)
            : base(propertyInfo.Name, propertyInfo.PropertyType, targetType, CreateSetValueAction(propertyInfo), targetRegistrationName)
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

        /// <inheritdoc />
        protected override InstanceInjection BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            return new PropertyInjection(closedGenericType, closedGenericTypeInfo.GetDeclaredProperty(PropertyInfo.Name), TargetRegistrationName);
        }
    }
}