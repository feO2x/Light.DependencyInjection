using System;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class FieldInjection : InstanceInjection
    {
        public readonly FieldInfo FieldInfo;

        public FieldInjection(FieldInfo fieldInfo, string childValueRegistrationName = null) 
            : base(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.DeclaringType, childValueRegistrationName)
        {
            CheckFieldInfo(fieldInfo);

            FieldInfo = fieldInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPublicSettableInstanceFieldInfo())
                return;

            throw new TypeRegistrationException($"The field info \"{fieldInfo}\" does not describe a public instance field.", fieldInfo.DeclaringType);
        }

        public override void InjectValue(object instance, object value)
        {
            FieldInfo.SetValue(instance, value);
        }

        protected override InstanceInjection CloneForBoundGenericTypeInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new FieldInjection(boundGenericTypeInfo.GetDeclaredField(FieldInfo.Name), ChildValueRegistrationName);
        }
    }
}