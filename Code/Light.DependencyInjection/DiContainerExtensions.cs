using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection
{
    public static class DiContainerExtensions
    {
        public static DiContainer RegisterTransient<T>(this DiContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, TransientLifetime.Instance);
        }

        public static DiContainer RegisterTransient(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, TransientLifetime.Instance);
        }

        public static DiContainer RegisterTransient<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, TransientLifetime.Instance, typeof(TAbstract));
        }

        public static DiContainer RegisterSingleton<T>(this DiContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new SingletonLifetime());
        }

        public static DiContainer RegisterSingleton(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new SingletonLifetime());
        }

        public static DiContainer RegisterSingleton<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new SingletonLifetime(), typeof(TAbstract));
        }

        public static DiContainer RegisterInstance(this DiContainer container, object instance, Action<IRegistrationOptionsForExternalInstance> configureOptions = null)
        {
            return RegistrationOptionsForExternalInstance.PerformRegistration(container, instance, configureOptions);
        }

        public static DiContainer RegisterScoped<T>(this DiContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance);
        }

        public static DiContainer RegisterScoped(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, ScopedLifetime.Instance);
        }

        public static DiContainer RegisterScoped<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance, typeof(TAbstract));
        }

        public static DiContainer RegisterPerThread<T>(this DiContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new PerThreadLifetime());
        }

        public static DiContainer RegisterPerThread(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new PerThreadLifetime());
        }

        public static DiContainer RegisterPerThread<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new PerThreadLifetime(), typeof(TAbstract));
        }

        public static DiContainer RegisterPerResolve<T>(this DiContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance);
        }

        public static DiContainer RegisterPerResolve(this DiContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, PerResolveLifetime.Instance);
        }

        public static DiContainer RegisterPerResolve<TAbstract, TConcrete>(this DiContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance, typeof(TAbstract));
        }
    }
}