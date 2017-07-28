using System;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection
{
    public static class DiContainerExtensions
    {
        public static DiContainer RegisterTransient<T>(this DiContainer container, Action<IRegistrationOptions<T>> configureRegistration = null)
        {
            return new RegistrationOptions<T>(container.Services.DefaultInstantiationInfoSelector).PerformRegistration(container, TransientLifetime.Instance, configureRegistration);
        }

        public static DiContainer RegisterSingleton<T>(this DiContainer container, Action<IRegistrationOptions<T>> configureRegistration = null)
        {
            return new RegistrationOptions<T>(container.Services.DefaultInstantiationInfoSelector).PerformRegistration(container, new SingletonLifetime(), configureRegistration);
        }

        public static DiContainer RegisterSingleton<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptions<TConcrete>> configureRegistration = null) where TConcrete : TAbstract
        {
            return new RegistrationOptions<TConcrete>(container.Services.DefaultInstantiationInfoSelector).MapToAbstractions(typeof(TAbstract))
                                                                                                          .PerformRegistration(container, new SingletonLifetime(), configureRegistration);
        }

        public static DiContainer RegisterInstance<T>(this DiContainer container, T instance)
        {
            return new RegistrationOptions<T>(container.Services.DefaultInstantiationInfoSelector).PerformRegistration(container, new ExternalInstanceLifetime(instance));
        }
    }
}