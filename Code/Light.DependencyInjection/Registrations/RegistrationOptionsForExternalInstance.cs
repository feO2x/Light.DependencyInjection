using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class RegistrationOptionsForExternalInstance : BaseRegistrationOptionsForExternalInstance<IRegistrationOptionsForExternalInstance>, IRegistrationOptionsForExternalInstance
    {
        public RegistrationOptionsForExternalInstance(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes) : base(targetType, ignoredAbstractionTypes) { }

        public void CreateAndAddRegistration(DiContainer container, object externalInstance)
        {
            container.MustNotBeNull(nameof(container));

            container.Register(new Registration(new TypeKey(TargetType, RegistrationName),
                                                new ExternalInstanceLifetime(externalInstance),
                                                IsContainerTrackingDisposables));
        }

        public static DiContainer PerformRegistration(DiContainer container, object externalInstance, Action<IRegistrationOptionsForExternalInstance> configureOptions)
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