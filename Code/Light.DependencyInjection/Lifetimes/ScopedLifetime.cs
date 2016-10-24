using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedLifetime : Lifetime
    {
        public static readonly ScopedLifetime Instance = new ScopedLifetime();

        public override object GetInstance(ResolveContext context)
        {
            return context.Container.Scope.GetOrAddScopedInstance(context.Registration.TypeKey,
                                                                  context.CreateInstance);
        }

        public override Lifetime ProvideInstanceForResolvedGenericTypeDefinition()
        {
            return this;
        }
    }
}