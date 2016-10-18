using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class FieldInjection : InstanceInjection
    {
        public readonly FieldInfo FieldInfo;

        public FieldInjection(FieldInfo fieldInfo, string targetRegistrationName = null)
            : base(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.DeclaringType, CreateSetValueAction(fieldInfo), targetRegistrationName)
        {
            CheckFieldInfo(fieldInfo);

            FieldInfo = fieldInfo;
        }

        private static Action<object, object> CreateSetValueAction(FieldInfo fieldInfo)
        {
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

        protected override InstanceInjection CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            return new FieldInjection(closedConstructedGenericTypeInfo.GetDeclaredField(FieldInfo.Name), TargetRegistrationName);
        }
    }
}