using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public interface ILifetime
    {
        object GetInstance(Registration registration, DiContainer container);
        object CreateInstance(Registration registration, DiContainer container, ParameterOverrides parameterOverrides);
        ILifetime ProvideInstanceForResolvedGenericType();
    }
}
