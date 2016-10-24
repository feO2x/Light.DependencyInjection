using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public abstract class Lifetime
    {
        private readonly string _toStringText;

        protected Lifetime()
        {
            _toStringText = GetType().Name;
        }

        public virtual bool RequiresTypeCreationInfo => true;
        public abstract object GetInstance(ResolveContext context);
        public abstract Lifetime ProvideInstanceForResolvedGenericTypeDefinition();

        public override string ToString()
        {
            return _toStringText;
        }
    }
}