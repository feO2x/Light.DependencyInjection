using System.Reflection;

namespace Light.DependencyInjection
{
    public interface IConstructorSelector
    {
        ConstructorInfo SelectTargetConstructor(TypeInfo typeInfo);
    }
}