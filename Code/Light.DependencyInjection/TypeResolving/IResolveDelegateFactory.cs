using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IResolveDelegateFactory
    {
        ResolveDelegate Create(TypeKey typeKey, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container);
        ResolveDelegate Create(Registration registration, DependencyOverrides dependencyOverrides, DependencyInjectionContainer container);
    }
}