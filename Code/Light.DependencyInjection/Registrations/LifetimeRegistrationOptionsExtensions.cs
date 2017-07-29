﻿using Light.DependencyInjection.Lifetimes;

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
    }
}