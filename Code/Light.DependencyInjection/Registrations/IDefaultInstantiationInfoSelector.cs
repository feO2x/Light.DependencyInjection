using System.Reflection;

namespace Light.DependencyInjection.Registrations
{
    public interface IDefaultInstantiationInfoSelector
    {
        InstantiationInfoFactory GetDefaultInstantiationInfo(TypeInfo typeInfo);
    }
}