using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Services
{
    /// <summary>
    ///     Represents a cluster of services that the <see cref="DependencyInjectionContainer" /> uses internally.
    /// </summary>
    public sealed class ContainerServices
    {
        private IConstructorSelector _constructorSelector = new ConstructorWithMostParametersSelector();
        private IContainerScopeFactory _containerScopeFactory = new DefaultContainerScopeFactory();
        private IDefaultRegistrationFactory _defaultRegistrationFactory = new TransientRegistrationFactory();
        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[] { typeof(IDisposable) };
        private IInjectorForUnknownInstanceMembers _injectorForUnknownInstanceMembers = new DefaultInjectorForUnknownInstanceMembers();
        private ResolveScopeFactory _resolveScopeFactory = new ResolveScopeFactory();

        /// <summary>
        ///     Gets or sets the service that selects a constructor from a concrete target type when the client did not specify any instantiation method explicitely.
        ///     Defaults to an instance of <see cref="ConstructorWithMostParametersSelector" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _constructorSelector = value;
            }
        }

        /// <summary>
        ///     Gets or sets the list containing all types that are ignored when abstraction types are mapped automatically to the concrete type.
        ///     Defaults to an array containing the <see cref="IDisposable" /> type.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IReadOnlyList<Type> IgnoredAbstractionTypes
        {
            get { return _ignoredAbstractionTypes; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _ignoredAbstractionTypes = value;
            }
        }

        /// <summary>
        ///     Gets or sets the service that is used to perform instance injections for type members that are unkown to the DI container (relevant for <see cref="DependencyOverrides" />).
        ///     Defaults to an instance of <see cref="DefaultInjectorForUnknownInstanceMembers" /> which performs property and field injection via reflection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IInjectorForUnknownInstanceMembers InjectorForUnknownInstanceMembers
        {
            get { return _injectorForUnknownInstanceMembers; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _injectorForUnknownInstanceMembers = value;
            }
        }

        /// <summary>
        ///     Gets or sets the factory that creates registrations for automatically resolved types.
        ///     Defaults to an instance of <see cref="TransientRegistrationFactory" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IDefaultRegistrationFactory DefaultRegistrationFactory
        {
            get { return _defaultRegistrationFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _defaultRegistrationFactory = value;
            }
        }

        /// <summary>
        ///     Gets or sets the factory that creates container scopes. Defaults to an instance of <see cref="DefaultContainerScopeFactory" />.
        ///     Note that the resulting scopes are not thread-safe by default. Thus, if you want to access the DI container from several threads,
        ///     you should exchange this value with an instance of <see cref="ThreadSafeContainerScopeFactory" />. However, this is not the recommended
        ///     approach in multi-threading scenarios - you should create a child container per thread / request.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IContainerScopeFactory ContainerScopeFactory
        {
            get { return _containerScopeFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _containerScopeFactory = value;
            }
        }

        /// <summary>
        ///     Gets or sets the factory that creates the lazy scope for each Resolve / ResolveAll.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ResolveScopeFactory ResolveScopeFactory
        {
            get { return _resolveScopeFactory; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _resolveScopeFactory = value;
            }
        }

        /// <summary>
        ///     Uses the <see cref="DefaultRegistrationFactory" /> to create a registration for the given type key.
        /// </summary>
        public Registration CreateDefaultRegistration(TypeKey typeKey)
        {
            var defaultOptions = CreateRegistrationOptions(typeKey.Type);
            defaultOptions.UseRegistrationName(typeKey.RegistrationName);
            return _defaultRegistrationFactory.CreateDefaultRegistration(defaultOptions.BuildTypeCreationInfo());
        }

        /// <summary>
        ///     Creates registration options for the specified type.
        /// </summary>
        public RegistrationOptionsForType CreateRegistrationOptions(Type targetType)
        {
            return new RegistrationOptionsForType(targetType, _constructorSelector, _ignoredAbstractionTypes);
        }

        /// <summary>
        ///     Creates registration options for the specified type.
        /// </summary>
        public RegistrationOptionsForType<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptionsForType<T>(_constructorSelector, _ignoredAbstractionTypes);
        }

        /// <summary>
        ///     Creates a shallow copy of this container services instance.
        /// </summary>
        /// <returns></returns>
        public ContainerServices Clone()
        {
            return (ContainerServices) MemberwiseClone();
        }
    }
}