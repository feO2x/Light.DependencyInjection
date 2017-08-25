using System;
using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ExternalInstanceLifetime : Lifetime
    {
        public readonly object Value;

        public ExternalInstanceLifetime(object value) : base(false, true)
        {
            Value = value.MustNotBeNull(nameof(value));
        }

        public override object ResolveInstance(IResolveContext resolveContext)
        {
            return Value;
        }

        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            throw new NotSupportedException("An external value cannot be registered as a generic type definition. This method must never been called on this lifetime.");
        }
    }
}