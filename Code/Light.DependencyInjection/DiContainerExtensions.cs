using System;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public static class DiContainerExtensions
    {
        public static DiContainer RegisterTransient<T>(this DiContainer diContainer, Action<IRegistrationOptions<T>> configureOptions)
        {
            diContainer.MustNotBeNull(nameof(diContainer));
            configureOptions.MustNotBeNull(nameof(configureOptions));

            var options = new RegistrationOptions<T>(diContainer.TypeAnalyzer.ConstructorSelector, diContainer.TypeAnalyzer.IgnoredAbstractionTypes);
            configureOptions(options);

            return diContainer.Register(new TransientRegistration(options.BuildTypeCreationInfo(), options.RegistrationName), options.MappedAbstractionTypes);
        }

        public static DiContainer RegisterTransient<T>(this DiContainer diContainer, string registrationName = null)
        {
            return RegisterTransient(diContainer, typeof(T), registrationName);
        }

        public static DiContainer RegisterTransient(this DiContainer diContainer, Type concreteType, string registrationName = null)
        {
            diContainer.MustNotBeNull(nameof(diContainer));
            concreteType.MustNotBeNull(nameof(concreteType));

            return diContainer.Register(new TransientRegistration(diContainer.TypeAnalyzer.CreateInfoFor(concreteType), registrationName));
        }

        public static DiContainer RegisterTransient(this DiContainer container, Type concreteType, Action<IRegistrationOptions> configureOptions)
        {
            container.MustNotBeNull(nameof(container));
            configureOptions.MustNotBeNull(nameof(configureOptions));

            var options = new RegistrationOptions(concreteType, container.TypeAnalyzer.ConstructorSelector, container.TypeAnalyzer.IgnoredAbstractionTypes);
            configureOptions(options);

            return container.Register(new TransientRegistration(options.BuildTypeCreationInfo(), options.RegistrationName), options.MappedAbstractionTypes);
        }

        public static DiContainer RegisterTransient<TAbstract, TConcrete>(this DiContainer diContainer, string registrationName = null) where TConcrete : TAbstract
        {
            diContainer.MustNotBeNull(nameof(diContainer));
            return diContainer.Register(new TransientRegistration(diContainer.TypeAnalyzer.CreateInfoFor(typeof(TConcrete)), registrationName), typeof(TAbstract));
        }

        public static DiContainer RegisterSingleton<T>(this DiContainer diContainer, string registrationName = null)
        {
            return RegisterSingleton(diContainer, typeof(T), registrationName);
        }

        public static DiContainer RegisterSingleton(this DiContainer diContainer, Type type, string registrationName = null)
        {
            diContainer.MustNotBeNull(nameof(diContainer));
            type.MustNotBeNull(nameof(type));

            return diContainer.Register(new SingletonRegistration(diContainer.TypeAnalyzer.CreateInfoFor(type), registrationName));
        }

        public static DiContainer RegisterInstance(this DiContainer diContainer, object instance, string registrationName = null)
        {
            diContainer.MustNotBeNull(nameof(diContainer));
            instance.MustNotBeNull(nameof(instance));

            return diContainer.Register(new ExternallyCreatedInstanceRegistration(instance, registrationName));
        }
    }
}