using Light.DependencyInjection.Lifetimes;

namespace Light.DependencyInjection.Registrations
{
    public static class LifetimeRegistrationOptionsExtensions
    {
        public static TOptions UseTransientLifetime<TOptions>(this TOptions registrationOptions) where TOptions : ICreateInstanceOptions<TOptions>
        {
            return registrationOptions.UseLifetime(TransientLifetime.Instance);
        }

        public static TOptions UseSingletonLifetime<TOptions>(this TOptions registrationOptions) where TOptions : ICreateInstanceOptions<TOptions>
        {
            return registrationOptions.UseLifetime(new SingletonLifetime());
        }

        public static TOptions UseScopedLifetime<TOptions>(this TOptions registrationOptions) where TOptions : ICreateInstanceOptions<TOptions>
        {
            return registrationOptions.UseLifetime(ScopedLifetime.Instance);
        }

        public static TOptions UseHierarchicalScopedLifetime<TOptions>(this TOptions registrationOptions) where TOptions : ICreateInstanceOptions<TOptions>
        {
            return registrationOptions.UseLifetime(HierarchicalScopedLifetime.Instance);
        }

        public static TOptions UsePerResolveLifetime<TOptions>(this TOptions registrationOptions) where TOptions : ICreateInstanceOptions<TOptions>
        {
            return registrationOptions.UseLifetime(PerResolveLifetime.Instance);
        }
    }
}