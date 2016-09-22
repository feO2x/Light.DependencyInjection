using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.Lifetimes
{
    public interface ILifetime
    {
        object GetInstance(Registration registration, DiContainer container);
    }
}
