using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public abstract class Lifetime
    {
        public virtual bool RequiresTypeCreationInfo => true;
        public abstract object GetInstance(ResolveContext context);
        public abstract Lifetime ProvideInstanceForResolvedGenericTypeDefinition();
    }
}