using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServices
    {
        private ICollectionFactory _collectionFactory = new ListFactory();
        private IConstructorSelector _constructorSelector = new ConstructorWithMostParametersSelector();
        private IContainerScopeFactory _containerScopeFactory = new DefaultContainerScopeFactory();
        private IDefaultRegistrationFactory _defaultRegistrationFactory = new TransientRegistrationFactory();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[] { typeof(IDisposable) };
        private IInjectorForUnknownInstanceMembers _injectorForUnknownInstanceMembers = new DefaultInjectorForUnknownInstanceMembers();

        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _constructorSelector = value;
            }
        }

        public IReadOnlyList<Type> IgnoredAbstractionTypes
        {
            get { return _ignoredAbstractionTypes; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _ignoredAbstractionTypes = value;
            }
        }

        public IInjectorForUnknownInstanceMembers InjectorForUnknownInstanceMembers
        {
            get { return _injectorForUnknownInstanceMembers; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _injectorForUnknownInstanceMembers = value;
            }
        }

        public IDefaultRegistrationFactory DefaultRegistrationFactory
        {
            get { return _defaultRegistrationFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _defaultRegistrationFactory = value;
            }
        }

        public IContainerScopeFactory ContainerScopeFactory
        {
            get { return _containerScopeFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _containerScopeFactory = value;
            }
        }

        public ICollectionFactory CollectionFactory
        {
            get { return _collectionFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _collectionFactory = value;
            }
        }

        public Registration CreateDefaultRegistration(TypeKey typeKey)
        {
            var defaultOptions = new RegistrationOptionsForType(typeKey.Type, _constructorSelector, _ignoredAbstractionTypes);
            defaultOptions.UseRegistrationName(typeKey.RegistrationName);
            return _defaultRegistrationFactory.CreateDefaultRegistration(defaultOptions.BuildTypeCreationInfo());
        }
    }
}