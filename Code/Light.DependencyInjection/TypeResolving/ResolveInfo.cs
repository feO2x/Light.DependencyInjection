using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public abstract class ResolveInfo
    {
        public readonly TypeKey RequestedTypeKey;

        protected ResolveInfo(TypeKey requestedTypeKey)
        {
            RequestedTypeKey = requestedTypeKey.MustNotBeEmpty(nameof(requestedTypeKey));
        }
    }
}