using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;

namespace Light.DependencyInjection.Lifetimes
{
    public sealed class ExternalInstanceLifetime : Lifetime
    {
        public readonly object Value;

        public ExternalInstanceLifetime(object value) : base(false)
        {
            Value = value.MustNotBeNull(nameof(value));
        }

        public override object ResolveInstance(ResolveContext resolveContext)
        {
            return Value;
        }
    }
}