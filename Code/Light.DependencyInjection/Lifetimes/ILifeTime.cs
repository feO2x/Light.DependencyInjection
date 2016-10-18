using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public interface ILifetime
    {
        object GetInstance(ResolveContext context);
        ILifetime ProvideInstanceForResolvedGenericType();
    }
}