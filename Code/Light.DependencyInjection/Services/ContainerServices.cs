using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    public sealed class ContainerServices
    {
        private IConstructorSelector _constructorSelector = new ConstructorWithMostParametersSelector();
        private IContainerScopeFactory _containerScopeFactory = new DefaultContainerScopeFactory();
        private IDefaultRegistrationFactory _defaultRegistrationFactory = new TransientRegistrationFactory();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[] { typeof(IDisposable) };
        private IInjectorForUnknownInstanceMembers _injectorForUnknownInstanceMembers = new DefaultInjectorForUnknownInstanceMembers();
        private ResolveScopeFactory _resolveScopeFactory = new ResolveScopeFactory();

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

        public ResolveScopeFactory ResolveScopeFactory
        {
            get { return _resolveScopeFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _resolveScopeFactory = value;
            }
        }

        public Registration CreateDefaultRegistration(TypeKey typeKey)
        {
            var defaultOptions = CreateRegistrationOptions(typeKey.Type);
            defaultOptions.UseRegistrationName(typeKey.RegistrationName);
            return _defaultRegistrationFactory.CreateDefaultRegistration(defaultOptions.BuildTypeCreationInfo());
        }

        public RegistrationOptionsForType CreateRegistrationOptions(Type targetType)
        {
            return new RegistrationOptionsForType(targetType, _constructorSelector, _ignoredAbstractionTypes);
        }

        public RegistrationOptionsForType<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptionsForType<T>(_constructorSelector, _ignoredAbstractionTypes);
        }

        public ContainerServices Clone()
        {
            return (ContainerServices) MemberwiseClone();
        }
    }
}