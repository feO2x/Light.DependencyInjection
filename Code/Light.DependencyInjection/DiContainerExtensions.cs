using System;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection
{
    public static class DiContainerExtensions
    {
        public static DependencyInjectionContainer RegisterTransient<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, TransientLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterTransient(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, TransientLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterTransient<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, TransientLifetime.Instance, typeof(TAbstract));
        }

        public static DependencyInjectionContainer RegisterSingleton<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new SingletonLifetime());
        }

        public static DependencyInjectionContainer RegisterSingleton(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new SingletonLifetime());
        }

        public static DependencyInjectionContainer RegisterSingleton<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new SingletonLifetime(), typeof(TAbstract));
        }

        public static DependencyInjectionContainer RegisterInstance(this DependencyInjectionContainer container, object instance, Action<IRegistrationOptionsForExternalInstance> configureOptions = null)
        {
            return RegistrationOptionsForExternalInstance.PerformRegistration(container, instance, configureOptions);
        }

        public static DependencyInjectionContainer RegisterScoped<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterScoped(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, ScopedLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterScoped<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance, typeof(TAbstract));
        }

        public static DependencyInjectionContainer RegisterPerThread<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new PerThreadLifetime());
        }

        public static DependencyInjectionContainer RegisterPerThread(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new PerThreadLifetime());
        }

        public static DependencyInjectionContainer RegisterPerThread<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new PerThreadLifetime(), typeof(TAbstract));
        }

        public static DependencyInjectionContainer RegisterPerResolve<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterPerResolve(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, PerResolveLifetime.Instance);
        }

        public static DependencyInjectionContainer RegisterPerResolve<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance, typeof(TAbstract));
        }
    }
}