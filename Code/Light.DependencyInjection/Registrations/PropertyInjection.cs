using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class PropertyInjection : InstanceManipulation
    {
        public readonly PropertyInfo TargetProperty;

        public PropertyInjection(TypeKey typeKey, PropertyInfo targetProperty, IReadOnlyList<Dependency> dependencies)
            : base(typeKey, targetProperty.MustNotBeNull(nameof(targetProperty)).Name, dependencies)
        {
            var foundProperty = TypeKey.Type.GetRuntimeProperty(targetProperty.Name);
            if (foundProperty == null || foundProperty.Equals(targetProperty) == false)
                throw new RegistrationException($"The type \"{TypeKey.Type}\" does not contain the property \"{targetProperty}\".", typeKey.Type);

            if (targetProperty.CanWrite == false)
                throw new RegistrationException($"The specified property \"{targetProperty}\" does not have a set method and thus cannot be used for Property Injection.", TypeKey.Type);

            if (targetProperty.SetMethod.IsStatic)
                throw new RegistrationException($"The specified property \"{targetProperty}\" is a static property and thus cannot be used for Property Injection.", TypeKey.Type);

            dependencies.MustHaveCount(1, nameof(dependencies));

            TargetProperty = targetProperty;
        }
    }
}