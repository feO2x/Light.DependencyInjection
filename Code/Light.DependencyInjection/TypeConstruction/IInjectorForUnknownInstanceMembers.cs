using System.Reflection;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IInjectorForUnknownInstanceMembers
    {
        void InjectValue(MemberInfo memberInfo, object instance, object value);
    }
}