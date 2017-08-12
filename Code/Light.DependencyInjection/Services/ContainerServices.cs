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
        public static readonly IReadOnlyList<Type> DefaultIgnoredAbstractionTypes = new[] { typeof(IDisposable) };
        public readonly IAutomaticRegistrationFactory AutomaticRegistrationFactory;

        public readonly IConcurrentDictionaryFactory ConcurrentDictionaryFactory;
        public readonly IConcurrentListFactory ConcurrentListFactory;
        public readonly IContainerScopeFactory ContainerScopeFactory;
        public readonly IDefaultInstantiationInfoSelector DefaultInstantiationInfoSelector;
        public readonly IReadOnlyList<Type> IgnoredAbstractionTypes;
        public readonly IResolveContextFactory ResolveContextFactory;
        public readonly IResolveDelegateFactory ResolveDelegateFactory;
        public readonly Action<DiContainer> SetupContainer;

        public ContainerServices(IConcurrentDictionaryFactory concurrentDictionaryFactory,
                                 IConcurrentListFactory concurrentListFactory,
                                 IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector,
                                 IReadOnlyList<Type> ignoredAbstractionTypes,
                                 IContainerScopeFactory containerScopeFactory,
                                 IResolveDelegateFactory resolveDelegateFactory,
                                 IAutomaticRegistrationFactory automaticRegistrationFactory,
                                 IResolveContextFactory resolveContextFactory,
                                 Action<DiContainer> setupContainer = null)
        {
            ConcurrentDictionaryFactory = concurrentDictionaryFactory.MustNotBeNull(nameof(concurrentDictionaryFactory));
            ConcurrentListFactory = concurrentListFactory.MustNotBeNull(nameof(concurrentListFactory));
            DefaultInstantiationInfoSelector = defaultInstantiationInfoSelector.MustNotBeNull(nameof(defaultInstantiationInfoSelector));
            IgnoredAbstractionTypes = ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));
            ContainerScopeFactory = containerScopeFactory.MustNotBeNull(nameof(containerScopeFactory));
            ResolveDelegateFactory = resolveDelegateFactory.MustNotBeNull(nameof(resolveDelegateFactory));
            AutomaticRegistrationFactory = automaticRegistrationFactory.MustNotBeNull(nameof(automaticRegistrationFactory));
            ResolveContextFactory = resolveContextFactory.MustNotBeNull(nameof(resolveContextFactory));
            SetupContainer = setupContainer;
        }

        public RegistrationOptions<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptions<T>(IgnoredAbstractionTypes, DefaultInstantiationInfoSelector);
        }

        public RegistrationOptions CreateRegistrationOptions(Type targetType)
        {
            return new RegistrationOptions(targetType, IgnoredAbstractionTypes, DefaultInstantiationInfoSelector);
        }

        public ExternalInstanceOptions CreateExternalInstanceOptions(object value)
        {
            return new ExternalInstanceOptions(value, IgnoredAbstractionTypes);
        }

        public static void DefaultSetupContainer(DiContainer container)
        {
            container.AddDefaultGuidRegistration()
                     .AddDefaultContainerRegistration();
        }

        public static CompiledLinqExpressionFactory CreateDefaultResolveDelegateFactory()
        {
            return new CompiledLinqExpressionFactory(new IInstantiationExpressionFactory[]
                                                     {
                                                         new ConstructorInstantiationExpressionFactory(),
                                                         new DelegateInstantiationExpressionFactory(),
                                                         new StaticMethodInstantiationExpressionFactory()
                                                     }.ToDictionary(expressionFactory => expressionFactory.InstantiationInfoType),
                                                     new IInstanceManipulationExpressionFactory[]
                                                     {
                                                         new PropertyInjectionExpressionFactory(),
                                                         new FieldInjectionExpressionFactory()
                                                     }.ToDictionary(expressionFactory => expressionFactory.InstanceManipulationType));
        }

        public ContainerServicesBuilder ToBuilder()
        {
            return new ContainerServicesBuilder(this);
        }
    }
}