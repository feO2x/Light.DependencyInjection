using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptionsForExternalInstances : BaseRegistrationOptionsForExternalInstances<IRegistrationOptionsForExternalInstances>, IRegistrationOptionsForExternalInstances
    {
        public RegistrationOptionsForExternalInstances(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes) : base(targetType, ignoredAbstractionTypes) { }

        public void CreateRegistration(DiContainer container, object externalInstance)
        {
            container.MustNotBeNull(nameof(container));

            container.Register(new Registration(new TypeKey(TargetType, RegistrationName),
                                                new ExternalInstanceLifetime(externalInstance),
                                                null,
                                                IsContainerTrackingDisposables));
        }

        public static DiContainer PerformRegistration(DiContainer container, object externalInstance, Action<IRegistrationOptionsForExternalInstances> configureOptions)
        {
            container.MustNotBeNull(nameof(container));
            externalInstance.MustNotBeNull(nameof(externalInstance));

            var options = new RegistrationOptionsForExternalInstances(externalInstance.GetType(), container.TypeAnalyzer.IgnoredAbstractionTypes);
            configureOptions?.Invoke(options);

            options.CreateRegistration(container, externalInstance);
            return container;
        }
    }
}