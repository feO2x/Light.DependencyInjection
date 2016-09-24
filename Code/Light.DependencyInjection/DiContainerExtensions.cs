using System;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection
{
    public static class DiContainerExtensions
    {
        public static DiContainer RegisterTransient<T>(this DiContainer container, Action<IRegistrationOptionsForTypes<T>> configureOptions = null)
        {
            return RegistrationOptionsForTypes<T>.PerformRegistration(container, configureOptions, TransientLifetime.Instance);
        }

        public static DiContainer RegisterTransient(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForTypes> configureOptions = null)
        {
            return RegistrationOptionsForTypes.PerformRegistration(container, concreteType, configureOptions, TransientLifetime.Instance);
        }

        public static DiContainer RegisterTransient<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForTypes<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForTypes<TConcrete>.PerformRegistration(container, configureOptions, TransientLifetime.Instance, typeof(TAbstract));
        }

        public static DiContainer RegisterSingleton<T>(this DiContainer container, Action<RegistrationOptionsForTypes<T>> configureOptions = null)
        {
            return RegistrationOptionsForTypes<T>.PerformRegistration(container, configureOptions, new SingletonLifetime());
        }

        public static DiContainer RegisterSingleton(this DiContainer container, Type concreteType, Action<RegistrationOptionsForTypes> configureOptions = null)
        {
            return RegistrationOptionsForTypes.PerformRegistration(container, concreteType, configureOptions, new SingletonLifetime());
        }

        public static DiContainer RegisterInstance(this DiContainer container, object instance, Action<IRegistrationOptionsForExternalInstances> configureOptions = null)
        {
            return RegistrationOptionsForExternalInstances.PerformRegistration(container, instance, configureOptions);
        }
    }
}