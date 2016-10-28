using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the default implementation of <see cref="IInjectorForUnknownInstanceMembers" /> that can perform property and field injection via reflection.
    /// </summary>
    public sealed class DefaultInjectorForUnknownInstanceMembers : IInjectorForUnknownInstanceMembers
    {
        /// <inheritdoc />
        public void InjectValue(MemberInfo memberInfo, object instance, object value)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.IsPublicSettableInstancePropertyInfo() == false)
                    throw new ResolveTypeException($"The specified property info \"{propertyInfo}\" does not describe a public settable instance property. Therefore, an injection for a member unknown to the DI container cannot be performed.", propertyInfo.DeclaringType);
                propertyInfo.SetValue(instance, value);
                return;
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                if (fieldInfo.IsPublicSettableInstanceFieldInfo() == false)
                    throw new ResolveTypeException($"The specified field info \"{fieldInfo}\" does not describe a public settable instance field. Therefore, an injection for a member unknown to the DI container cannot be performed.", fieldInfo.DeclaringType);
                fieldInfo.SetValue(instance, value);
                return;
            }

            throw new ResolveTypeException($"The DI container is not able to perform an injection on the unknown member {memberInfo}. Please provide another implementation than {nameof(DefaultInjectorForUnknownInstanceMembers)}.", memberInfo.DeclaringType);
        }
    }
}