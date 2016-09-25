using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : ILifetime
    {
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        public object GetInstance(ResolveContext context)
        {
            return context.Container.Scope.GetOrAddInstance(context.Registration.TypeKey,
                                                            context.CreateInstance);
        }

        public ILifetime ProvideInstanceForResolvedGenericType()
        {
            return this;
        }
    }
}