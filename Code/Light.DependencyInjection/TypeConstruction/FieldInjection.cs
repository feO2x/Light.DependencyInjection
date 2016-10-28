using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="InstanceInjection" /> that injects a value into a field of the target instance.
    /// </summary>
    public sealed class FieldInjection : InstanceInjection
    {
        /// <summary>
        ///     Gets the info of the field that is populated.
        /// </summary>
        public readonly FieldInfo FieldInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="FieldInjection" />.
        /// </summary>
        /// <param name="fieldInfo">The info of the property that should be populated.</param>
        /// <param name="targetRegistrationName">The name of the target registration whose resolved value should be injected into the field (optional).</param>
        public FieldInjection(FieldInfo fieldInfo, string targetRegistrationName = null)
            : base(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.DeclaringType, CreateSetValueAction(fieldInfo), targetRegistrationName)
        {
            CheckFieldInfo(fieldInfo);

            FieldInfo = fieldInfo;
        }

        private static Action<object, object> CreateSetValueAction(FieldInfo fieldInfo)
        {
            if (fieldInfo.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var instanceDowncast = Expression.Convert(instanceParameter, fieldInfo.DeclaringType);
            var valueDowncast = Expression.Convert(valueParameter, fieldInfo.FieldType);

            var assignField = Expression.Assign(Expression.Field(instanceDowncast, fieldInfo), valueDowncast);
            return Expression.Lambda<Action<object, object>>(assignField, instanceParameter, valueParameter).Compile();
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublicSettableInstanceFieldInfo())
                return;

            throw new TypeRegistrationException($"The field info \"{fieldInfo}\" does not describe a public instance field.", fieldInfo.DeclaringType);
        }

        /// <inheritdoc />
        protected override InstanceInjection BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            return new FieldInjection(closedGenericTypeInfo.GetDeclaredField(FieldInfo.Name), TargetRegistrationName);
        }
    }
}