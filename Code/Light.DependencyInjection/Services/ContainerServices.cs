using System;
using System.Collections.Generic;
using System.Linq;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServices
    {
        private IConcurrentDictionaryFactory _concurrentDictionaryFactory = new DefaultConcurrentDictionaryFactory();
        private IConcurrentListFactory _concurrentListFactory = new ReaderWriterLockedListFactory();
        private IDefaultInstantiationInfoSelector _defaultInstantiationInfoSelector = new ConstructorWithMostParametersSelector();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[] { typeof(IDisposable) };
        private IContainerScopeFactory _containerScopeFactory = new DefaultContainerScopeFactory();

        private IResolveDelegateFactory _resolveDelegateFactory = new DefaultResolveDelegateFactory(new IInstantiationExpressionFactory[]
                                                                                                    {
                                                                                                        new ConstructorInstantiationExpressionFactory()
                                                                                                    }.ToDictionary(expressionFactory => expressionFactory.InstantiationInfoType));

        public IConcurrentDictionaryFactory ConcurrentDictionaryFactory
        {
            get => _concurrentDictionaryFactory;
            set => _concurrentDictionaryFactory = value.MustNotBeNull();
        }

        public IResolveDelegateFactory ResolveDelegateFactory
        {
            get => _resolveDelegateFactory;
            set => _resolveDelegateFactory = value.MustNotBeNull();
        }

        public IReadOnlyList<Type> IgnoredAbstractionTypes
        {
            get => _ignoredAbstractionTypes;
            set => _ignoredAbstractionTypes = value.MustNotBeNull();
        }

        public IDefaultInstantiationInfoSelector DefaultInstantiationInfoSelector
        {
            get => _defaultInstantiationInfoSelector;
            set => _defaultInstantiationInfoSelector = value.MustNotBeNull();
        }

        public IConcurrentListFactory ConcurrentListFactory
        {
            get => _concurrentListFactory;
            set => _concurrentListFactory = value.MustNotBeNull();
        }

        public IContainerScopeFactory ContainerScopeFactory
        {
            get => _containerScopeFactory;
            set => _containerScopeFactory = value.MustNotBeNull();
        }

        public RegistrationOptions<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptions<T>(_ignoredAbstractionTypes, _defaultInstantiationInfoSelector);
        }

        public RegistrationOptions CreateRegistrationOptions(Type targetType)
        {
            return new RegistrationOptions(targetType, _ignoredAbstractionTypes, _defaultInstantiationInfoSelector);
        }

        public ExternalInstanceOptions CreateExternalInstanceOptions(object value)
        {
            return new ExternalInstanceOptions(value, _ignoredAbstractionTypes);
        }
    }
}