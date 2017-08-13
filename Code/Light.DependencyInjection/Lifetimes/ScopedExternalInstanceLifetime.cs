using System;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ScopedExternalInstanceLifetime : Lifetime
    {
        public readonly TypeKey TypeKey;

        public ScopedExternalInstanceLifetime(TypeKey typeKey)
        {
            TypeKey = typeKey.MustNotBeEmpty(nameof(typeKey));
        }

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            if (resolveContext.TryGetPerResolveInstance(TypeKey, out var instance))
                return instance;

            throw new ResolveException($"There is no external instance available in the current scope for {TypeKey}.");
        }

        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            throw new NotSupportedException("An external value cannot be registered as a generic type definition. This method must never been called on this lifetime.");
        }
    }
}