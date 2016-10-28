using System.Reflection;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the abstraction of a service that is able to inject values into instance members where no <see cref="InstanceInjection" /> was configured.
    /// </summary>
    public interface IInjectorForUnknownInstanceMembers
    {
        /// <summary>
        ///     Injects the specified value into the member of the given instance.
        /// </summary>
        void InjectValue(MemberInfo memberInfo, object instance, object value);
    }
}