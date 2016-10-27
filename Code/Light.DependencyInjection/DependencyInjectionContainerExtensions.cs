using System;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection
{
    /// <summary>
    ///     Provides extension methods to easily register types with the <see cref="DependencyInjectionContainer" /> using a fluent API.
    /// </summary>
    public static class DependencyInjectionContainerExtensions
    {
        /// <summary>
        ///     Registers the specified concrete type with a transient lifetime.
        /// </summary>
        /// <typeparam name="T">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when T is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterTransient<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, TransientLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a transient lifetime.
        /// </summary>
        /// <param name="concreteType">The concrete type to be registered.</param>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="concreteType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="concreteType" /> is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterTransient(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, TransientLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a transient lifetime and maps it to the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that the concrete one should be mapped to.</typeparam>
        /// <typeparam name="TConcrete">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when TConcrete is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterTransient<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, TransientLifetime.Instance, typeof(TAbstract));
        }

        /// <summary>
        ///     Registers the specified concrete type with a singleton lifetime.
        /// </summary>
        /// <typeparam name="T">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when T is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterSingleton<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new SingletonLifetime());
        }

        /// <summary>
        ///     Registers the specified concrete type with a singleton lifetime.
        /// </summary>
        /// <param name="concreteType">The concrete type to be registered.</param>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="concreteType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="concreteType" /> is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterSingleton(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new SingletonLifetime());
        }

        /// <summary>
        ///     Registers the specified concrete type with a singleton lifetime and maps it to the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that the concrete one should be mapped to.</typeparam>
        /// <typeparam name="TConcrete">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when TConcrete is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterSingleton<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new SingletonLifetime(), typeof(TAbstract));
        }

        /// <summary>
        ///     Registers the specified instance as a singleton with the DI container.
        /// </summary>
        /// <param name="container">The target container.</param>
        /// <param name="instance">The instance to be registered.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="instance" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterInstance(this DependencyInjectionContainer container, object instance, Action<IRegistrationOptionsForExternalInstance> configureOptions = null)
        {
            return RegistrationOptionsForExternalInstance.PerformRegistration(container, instance, configureOptions);
        }

        /// <summary>
        ///     Registers the specified concrete type with a scoped lifetime.
        /// </summary>
        /// <typeparam name="T">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when T is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterScoped<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a scoped lifetime.
        /// </summary>
        /// <param name="concreteType">The concrete type to be registered.</param>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="concreteType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="concreteType" /> is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterScoped(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, ScopedLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a scoped lifetime and maps it to the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that the concrete one should be mapped to.</typeparam>
        /// <typeparam name="TConcrete">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when TConcrete is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterScoped<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, ScopedLifetime.Instance, typeof(TAbstract));
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-thread lifetime.
        /// </summary>
        /// <typeparam name="T">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when T is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerThread<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, new PerThreadLifetime());
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-thread lifetime.
        /// </summary>
        /// <param name="concreteType">The concrete type to be registered.</param>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="concreteType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="concreteType" /> is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerThread(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, new PerThreadLifetime());
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-thread lifetime and maps it to the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that the concrete one should be mapped to.</typeparam>
        /// <typeparam name="TConcrete">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when TConcrete is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerThread<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, new PerThreadLifetime(), typeof(TAbstract));
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-resolve lifetime.
        /// </summary>
        /// <typeparam name="T">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when T is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerResolve<T>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<T>> configureOptions = null)
        {
            return RegistrationOptionsForType<T>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-resolve lifetime.
        /// </summary>
        /// <param name="concreteType">The concrete type to be registered.</param>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="concreteType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when <paramref name="concreteType" /> is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerResolve(this DependencyInjectionContainer container, Type concreteType, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            return RegistrationOptionsForType.PerformRegistration(container, concreteType, configureOptions, PerResolveLifetime.Instance);
        }

        /// <summary>
        ///     Registers the specified concrete type with a per-resolve lifetime and maps it to the abstract type.
        /// </summary>
        /// <typeparam name="TAbstract">The abstract type that the concrete one should be mapped to.</typeparam>
        /// <typeparam name="TConcrete">The concrete type to be registered.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="configureOptions">The delegate used to configure the registration options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when TConcrete is not a instantiable concrete type or when the options were not configured properly.</exception>
        public static DependencyInjectionContainer RegisterPerResolve<TAbstract, TConcrete>(this DependencyInjectionContainer container, Action<IRegistrationOptionsForType<TConcrete>> configureOptions = null)
            where TConcrete : TAbstract
        {
            return RegistrationOptionsForType<TConcrete>.PerformRegistration(container, configureOptions, PerResolveLifetime.Instance, typeof(TAbstract));
        }
    }
}