using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class FieldInjection : InstanceManipulation
    {
        public readonly FieldInfo TargetField;

        public FieldInjection(TypeKey typeKey, FieldInfo targetField, IReadOnlyList<Dependency> dependencies)
            : base(typeKey, targetField.MustNotBeNull(nameof(targetField)).Name, dependencies)
        {
            var foundField = TypeKey.Type.GetRuntimeField(targetField.Name);
            if (foundField == null || foundField.Name != targetField.Name || foundField.FieldType.IsEquivalentTo(targetField.FieldType) == false)
                throw new RegistrationException($"The type \"{TypeKey.Type}\" does not contain the field \"{targetField}\".");

            if (foundField.IsStatic)
                throw new RegistrationException($"The specified field \"{targetField}\" is a static field and thus cannot be used for Field Injection.");

            dependencies.MustHaveCount(1, nameof(dependencies));
            TargetField = foundField;
        }
    }
}