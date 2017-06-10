using System;
using System.Collections.Generic;
using System.Linq;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServices
    {
        private IConcurrentDictionaryFactory _concurrentDictionaryFactory = new DefaultConcurrentDictionaryFactory();
        private IConcurrentListFactory _concurrentListFactory = new ReaderWriterLockedListFactory();
        private IDefaultInstantiationInfoSelector _defaultInstantiationInfoSelector = new ConstructorWithMostParametersSelector();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[] { typeof(IDisposable) };

        private IStandardizedConstructionFunctionFactory _standardizedConstructionFunctionFactory = new DefaultStandardizedConstructionFunctionFactory(new IInstantiationExpressionFactory[]
                                                                                                                                                       {
                                                                                                                                                           new ConstructorInstantiationExpressionFactory()
                                                                                                                                                       }.ToDictionary(expressionFactory => expressionFactory.InstantiationInfoType));

        public IConcurrentDictionaryFactory ConcurrentDictionaryFactory
        {
            get => _concurrentDictionaryFactory;
            set => _concurrentDictionaryFactory = value.MustNotBeNull();
        }

        public IStandardizedConstructionFunctionFactory StandardizedConstructionFunctionFactory
        {
            get => _standardizedConstructionFunctionFactory;
            set => _standardizedConstructionFunctionFactory = value.MustNotBeNull();
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
    }
}