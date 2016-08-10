using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class FieldInjection : InstanceInjection
    {
        private readonly FieldInfo _fieldInfo;
        public FieldInjection(FieldInfo fieldInfo, string childValueRegistrationName = null) : base(fieldInfo.Name, fieldInfo.FieldType, childValueRegistrationName)
        {
            CheckFieldInfo(fieldInfo);

            _fieldInfo = fieldInfo;
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
            _fieldInfo.SetValue(instance, value);
        }
    }
}