using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public interface ILifetime
    {
        object GetInstance(ResolveContext context);
        ILifetime ProvideInstanceForResolvedGenericType();
    }
}