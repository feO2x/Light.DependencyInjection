using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IResolveDelegateFactory
    {
        ResolveDelegate Create(TypeKey typeKey, DiContainer container);
    }
}