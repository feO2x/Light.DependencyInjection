using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IResolveContext
    {
        DependencyInjectionContainer Container { get; }
        Registration Registration { get; }

        object CreateInstance();

        bool TryGetPerResolveInstance(TypeKey typeKey, out object instance);
        bool GetOrCreatePerResolveInstance(TypeKey typeKey, out object instance);
        object GetOrCreatePerResolveInstance(TypeKey typeKey);
    }
}