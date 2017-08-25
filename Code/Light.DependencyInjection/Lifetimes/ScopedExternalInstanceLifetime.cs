using System;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedExternalInstanceLifetime : Lifetime
    {
        public static readonly ScopedExternalInstanceLifetime Instance = new ScopedExternalInstanceLifetime();

        public ScopedExternalInstanceLifetime() : base(false) { }

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            if (resolveContext.Container.Scope.TryGetScopedInstance(resolveContext.Registration.TypeKey, out var instance))
                return instance;

            throw new ResolveException($"There is no external instance available in the current scope for {resolveContext.Registration.TypeKey}.");
        }

        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            throw new NotSupportedException("An external value cannot be registered as a generic type definition. This method must never been called on this lifetime.");
        }
    }
}