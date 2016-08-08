using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class FieldInjection : InstanceInjection
    {
        private readonly FieldInfo _fieldInfo;
        public FieldInjection(FieldInfo fieldInfo) : base(fieldInfo.Name, fieldInfo.FieldType)
        {
            CheckFieldInfo(fieldInfo);

            _fieldInfo = fieldInfo;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic == false &&
                fieldInfo.IsPublic)
                return;

            throw new TypeRegistrationException($"The field info \"{fieldInfo}\" does not describe a public instance field.", fieldInfo.DeclaringType);
        }

        public override void InjectValue(object instance, object value)
        {
            _fieldInfo.SetValue(instance, value);
        }
    }
}