using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the options for configuring registrations that return an externally created instance (i.e. the DI container does not create instances of the corresponding type).
    /// </summary>
    public sealed class RegistrationOptionsForExternalInstance : BaseRegistrationOptionsForExternalInstance<IRegistrationOptionsForExternalInstance>, IRegistrationOptionsForExternalInstance
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="RegistrationOptionsForExternalInstance" />.
        /// </summary>
        /// <param name="targetType">The type of the external instance.</param>
        /// <param name="ignoredAbstractionTypes">The abstraction types that will be ignored when the target type is mapped to abstraction types.</param>
        public RegistrationOptionsForExternalInstance(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes) : base(targetType, ignoredAbstractionTypes) { }

        /// <summary>
        ///     Creates a <see cref="Registration" /> instance out of this options instance and registers it with the target container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <param name="externalInstance">The object that will be registered with the DI container.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> is null.</exception>
        public void CreateAndAddRegistration(DependencyInjectionContainer container, object externalInstance)
        {
            container.MustNotBeNull(nameof(container));

            container.Register(new Registration(new TypeKey(TargetType, RegistrationName),
                                                new ExternalInstanceLifetime(externalInstance),
                                                IsContainerTrackingDisposables),
                               AbstractionTypes);
        }

        /// <summary>
        ///     Creates a <see cref="Registration" /> instance out of these options and registers it with the target container.
        /// </summary>
        /// <param name="container">The container that will be populated.</param>
        /// <param name="externalInstance">The object that will be registered with the DI container.</param>
        /// <param name="configureOptions">The delegate that configures an instance of these options.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container" /> or <paramref name="externalInstance" /> is null.</exception>
        public static DependencyInjectionContainer PerformRegistration(DependencyInjectionContainer container, object externalInstance, Action<IRegistrationOptionsForExternalInstance> configureOptions)
        {
            container.MustNotBeNull(nameof(container));
            externalInstance.MustNotBeNull(nameof(externalInstance));

            var options = new RegistrationOptionsForExternalInstance(externalInstance.GetType(), container.Services.IgnoredAbstractionTypes);
            configureOptions?.Invoke(options);

            options.CreateAndAddRegistration(container, externalInstance);
            return container;
        }
    }
}