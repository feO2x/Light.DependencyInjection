using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IResolveInfoAlgorithm
    {
        ResolveInfo Search(TypeKey requestedTypeKey, DiContainer container, bool? tryResolveAll);
    }
}