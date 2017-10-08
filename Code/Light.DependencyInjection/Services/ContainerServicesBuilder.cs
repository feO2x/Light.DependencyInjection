using System;
using System.Collections.Generic;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Threading;
using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServicesBuilder
    {
        private IAutomaticRegistrationFactory _automaticRegistrationFactory;
        private IConcurrentDictionaryFactory _concurrentDictionaryFactory;
        private IContainerScopeFactory _containerScopeFactory;
        private IDefaultInstantiationInfoSelector _defaultInstantiationInfoSelector;
        private IReadOnlyList<Type> _ignoredAbstractionTypes;
        private IRegistrationCollectionFactory _registrationCollectionFactory;
        private IResolveContextFactory _resolveContextFactory;
        private IResolveDelegateFactory _resolveDelegateFactory;
        private IResolveInfoAlgorithm _resolveInfoAlgorithm;
        private Action<DependencyInjectionContainer> _setupContainer;

        public ContainerServicesBuilder()
        {
            _automaticRegistrationFactory = new DefaultAutomaticRegistrationFactory();
            _concurrentDictionaryFactory = new DefaultConcurrentDictionaryFactory();
            _registrationCollectionFactory = new ReaderWriterLockedListFactory();
            _containerScopeFactory = new DefaultContainerScopeFactory();
            _defaultInstantiationInfoSelector = new ConstructorWithMostParametersSelector();
            _ignoredAbstractionTypes = ContainerServices.DefaultIgnoredAbstractionTypes;
            _resolveContextFactory = new PerThreadResolveContextFactory();
            _resolveDelegateFactory = ContainerServices.CreateDefaultResolveDelegateFactory();
            _resolveInfoAlgorithm = new PreferResolveAllOverGenericTypeDefinitionAlgorithm();
            _setupContainer = ContainerServices.DefaultSetupContainer;
        }

        public ContainerServicesBuilder(ContainerServices existingServices)
        {
            existingServices.MustNotBeNull(nameof(existingServices));

            _concurrentDictionaryFactory = existingServices.ConcurrentDictionaryFactory;
            _registrationCollectionFactory = existingServices.RegistrationCollectionFactory;
            _containerScopeFactory = existingServices.ContainerScopeFactory;
            _defaultInstantiationInfoSelector = existingServices.DefaultInstantiationInfoSelector;
            _ignoredAbstractionTypes = existingServices.IgnoredAbstractionTypes;
            _resolveDelegateFactory = existingServices.ResolveDelegateFactory;
            _automaticRegistrationFactory = existingServices.AutomaticRegistrationFactory;
            _resolveContextFactory = existingServices.ResolveContextFactory;
            _resolveInfoAlgorithm = existingServices.ResolveInfoAlgorithm;
            _setupContainer = existingServices.SetupContainer;
        }

        public ContainerServicesBuilder WithConcurrentDictionaryFactory(IConcurrentDictionaryFactory concurrentDictionaryFactory)
        {
            _concurrentDictionaryFactory = concurrentDictionaryFactory;
            return this;
        }

        public ContainerServicesBuilder WithConcurrenteListFactory(IRegistrationCollectionFactory registrationCollectionFactory)
        {
            _registrationCollectionFactory = registrationCollectionFactory;
            return this;
        }

        public ContainerServicesBuilder WithContainerScopeFactory(IContainerScopeFactory containerScopeFactory)
        {
            _containerScopeFactory = containerScopeFactory;
            return this;
        }

        public ContainerServicesBuilder WithDefaultInstantiationInfoSelector(IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector)
        {
            _defaultInstantiationInfoSelector = defaultInstantiationInfoSelector;
            return this;
        }

        public ContainerServicesBuilder WithIgnoredAbstractionTypes(IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            _ignoredAbstractionTypes = ignoredAbstractionTypes;
            return this;
        }

        public ContainerServicesBuilder WithResolveDelegateFactory(IResolveDelegateFactory resolveDelegateFactory)
        {
            _resolveDelegateFactory = resolveDelegateFactory;
            return this;
        }

        public ContainerServicesBuilder WithSetupContainer(Action<DependencyInjectionContainer> setupContainer)
        {
            _setupContainer = setupContainer;
            return this;
        }

        public ContainerServicesBuilder PerformNoContainerSetup()
        {
            _setupContainer = null;
            return this;
        }

        public ContainerServicesBuilder WithAutomaticRegistrationFactory(IAutomaticRegistrationFactory automaticRegistrationFactory)
        {
            _automaticRegistrationFactory = automaticRegistrationFactory;
            return this;
        }

        public ContainerServicesBuilder DisallowAutomaticRegistrations()
        {
            _automaticRegistrationFactory = new NoAutoRegistrationsAllowedFactory();
            return this;
        }

        public ContainerServicesBuilder WithResolveContextFactory(IResolveContextFactory resolveContextFactory)
        {
            _resolveContextFactory = resolveContextFactory;
            return this;
        }

        public ContainerServicesBuilder WithResolveInfoAlgorithm(IResolveInfoAlgorithm algorithm)
        {
            _resolveInfoAlgorithm = algorithm;
            return this;
        }

        public ContainerServices Build()
        {
            return new ContainerServices(_concurrentDictionaryFactory,
                                         _registrationCollectionFactory,
                                         _defaultInstantiationInfoSelector,
                                         _ignoredAbstractionTypes,
                                         _containerScopeFactory,
                                         _resolveDelegateFactory,
                                         _automaticRegistrationFactory,
                                         _resolveContextFactory,
                                         _resolveInfoAlgorithm,
                                         _setupContainer);
        }
    }
}