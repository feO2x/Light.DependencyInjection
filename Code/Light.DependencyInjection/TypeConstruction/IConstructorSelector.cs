using System.Reflection;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IConstructorSelector
    {
        ConstructorInfo SelectTargetConstructor(TypeInfo typeInfo);
    }
}