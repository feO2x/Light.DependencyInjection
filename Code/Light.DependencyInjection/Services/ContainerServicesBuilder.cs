using System;
using System.Collections.Generic;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServicesBuilder
    {
        private IConcurrentDictionaryFactory _concurrentDictionaryFactory = new DefaultConcurrentDictionaryFactory();
        private IConcurrentListFactory _concurrentListFactory = new ReaderWriterLockedListFactory();
        private IContainerScopeFactory _containerScopeFactory = new DefaultContainerScopeFactory();
        private IDefaultInstantiationInfoSelector _defaultInstantiationInfoSelector = new ConstructorWithMostParametersSelector();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = ContainerServices.DefaultIgnoredAbstractionTypes;
        private IResolveDelegateFactory _resolveDelegateFactory = ContainerServices.CreateDefaultResolveDelegateFactory();
        private Action<DiContainer> _setupContainer = ContainerServices.DefaultSetupContainer;
        private IAutomaticRegistrationFactory _automaticRegistrationFactory = new DefaultAutomaticRegistrationFactory();

        public ContainerServicesBuilder WithConcurrentDictionaryFactory(IConcurrentDictionaryFactory concurrentDictionaryFactory)
        {
            _concurrentDictionaryFactory = concurrentDictionaryFactory;
            return this;
        }

        public ContainerServicesBuilder WithConcurrenteListFactory(IConcurrentListFactory concurrentListFactory)
        {
            _concurrentListFactory = concurrentListFactory;
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

        public ContainerServicesBuilder WithSetupContainer(Action<DiContainer> setupContainer)
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

        public ContainerServices Build()
        {
            return new ContainerServices(_concurrentDictionaryFactory,
                                         _concurrentListFactory,
                                         _defaultInstantiationInfoSelector,
                                         _ignoredAbstractionTypes,
                                         _containerScopeFactory,
                                         _resolveDelegateFactory,
                                         _automaticRegistrationFactory,
                                         _setupContainer);
        }
    }
}