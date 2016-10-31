using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    /// <summary>
    ///     Represents a dependendency injection container that resolves object graphs recursively.
    ///     Use the fluent API of <see cref="DependencyInjectionContainerExtensions" /> to easily
    ///     configure registrations with the DI container.
    /// </summary>
    public class DependencyInjectionContainer : IServiceProvider, IDisposable
    {
        private static readonly Type DiContainerType = typeof(DependencyInjectionContainer);
        private static readonly Type ServiceProviderType = typeof(IServiceProvider);
        private static readonly Type GuidType = typeof(Guid);
        private readonly ThreadLocal<Dictionary<TypeKey, object>> _overriddenMappings = new ThreadLocal<Dictionary<TypeKey, object>>(() => new Dictionary<TypeKey, object>());
        private readonly RegistrationDictionary _typeMappings;

        /// <summary>
        ///     Gets the scope for this container instance.
        /// </summary>
        public readonly ContainerScope Scope;

        private ContainerServices _services;

        /// <summary>
        ///     Initializes a new instance of <see cref="DependencyInjectionContainer" />.
        /// </summary>
        public DependencyInjectionContainer() : this(new ContainerServices()) { }

        /// <summary>
        ///     Initializes a new instance of <see cref="DependencyInjectionContainer" />.
        /// </summary>
        /// <param name="containerServices">The object that provides access to services that the container uses internally.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="containerServices" /> is null.</exception>
        public DependencyInjectionContainer(ContainerServices containerServices)
            : this(new RegistrationDictionary(), containerServices) { }

        private DependencyInjectionContainer(RegistrationDictionary typeMappings,
                                             ContainerServices services,
                                             ContainerScope parentScope = null)
        {
            typeMappings.MustNotBeNull(nameof(typeMappings));
            services.MustNotBeNull(nameof(services));

            _typeMappings = typeMappings;
            _services = services;
            Scope = services.ContainerScopeFactory.CreateScope(parentScope);
        }

        /// <summary>
        ///     Gets a list of all registrations.
        /// </summary>
        public IReadOnlyList<Registration> Registrations => _typeMappings.Registrations;

        /// <summary>
        ///     Gets the dictionary containing all mappings that will override existing mappings and registrations on the next call to Resolve / ResolveAll.
        /// </summary>
        public IReadOnlyDictionary<TypeKey, object> OverriddenMappings => _overriddenMappings.Value;

        /// <summary>
        ///     Gets or sets the container services that are used internally.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public ContainerServices Services
        {
            get { return _services; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _services = value;
            }
        }

        /// <summary>
        ///     Disposes of the DI container and all <see cref="IDisposable" /> instances that the container tracks.
        /// </summary>
        public void Dispose()
        {
            Scope.Dispose();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        /// <summary>
        ///     Creates a new child container with a child scope connected to this container's scope.
        /// </summary>
        public DependencyInjectionContainer CreateChildContainer()
        {
            return CreateChildContainer(ChildContainerOptions.Default);
        }

        /// <summary>
        ///     Creates a new child container with the specified options.
        /// </summary>
        /// <param name="options">The options specifying how scope, type mappings, and container services are shared with the new child container.</param>
        public DependencyInjectionContainer CreateChildContainer(ChildContainerOptions options)
        {
            var parentScope = options.CreateEmptyScope ? null : Scope;
            var typeMappings = options.CreateCopyOfMappings ? new RegistrationDictionary(_typeMappings) : _typeMappings;
            var services = options.CloneContainerServices ? _services.Clone() : _services;

            return new DependencyInjectionContainer(typeMappings, services, parentScope);
        }

        /// <summary>
        ///     Registers the specified registration with the DI container, mapping all given abstractions to the registration's concrete type.
        /// </summary>
        /// <param name="registration">The registration describing how a concrete type should be resolved.</param>
        /// <param name="abstractionTypes">The abstraction types that should be mapped to the registration's concrete type.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public DependencyInjectionContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            registration.MustNotBeNull(nameof(registration));
            abstractionTypes.MustNotBeNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _typeMappings.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
            }
            // ReSharper restore PossibleMultipleEnumeration

            Register(registration);
            return this;
        }

        /// <summary>
        ///     Registers the specified registration with the DI container, mapping all given abstractions to the registration's concrete type.
        /// </summary>
        /// <param name="registration">The registration describing how a concrete type should be resolved.</param>
        /// <param name="abstractionTypes">The abstraction types that should be mapped to the registration's concrete type.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public DependencyInjectionContainer Register(Registration registration, params Type[] abstractionTypes)
        {
            return Register(registration, (IEnumerable<Type>) abstractionTypes);
        }

        /// <summary>
        ///     Registers the specified registration with the DI container.
        /// </summary>
        /// <param name="registration">The registration describing how a concrete type should be resolved.</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registration" /> is null.</exception>
        public DependencyInjectionContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _typeMappings.AddOrReplace(new TypeKey(registration.TargetType, registration.Name), registration);

            return this;
        }

        /// <summary>
        ///     Resolves the object graph for the given type, with an optional registration name.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The instantiated object graph.</returns>
        /// <exception cref="ResolveTypeException">Thrown when the given type could not be resolved properly.</exception>
        public T Resolve<T>(string registrationName = null)
        {
            return (T) PerformResolve(new TypeKey(typeof(T), registrationName),
                                      ResolveContext.CreateInitial(this));
        }

        /// <summary>
        ///     Resolves the object graph for the given type, with an optional registration name.
        /// </summary>
        /// <param name="type">The type of the object graph root.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The instantiated object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="ResolveTypeException">Thrown when the given type could not be resolved properly.</exception>
        public object Resolve(Type type, string registrationName = null)
        {
            return PerformResolve(new TypeKey(type, registrationName),
                                  ResolveContext.CreateInitial(this));
        }

        /// <summary>
        ///     Resolves the object graph for the given type, with an optional registration name. Overrides dependencies
        ///     with the specified <paramref name="dependencyOverrides" />.
        /// </summary>
        /// <typeparam name="T">The type of the object graph root.</typeparam>
        /// <param name="dependencyOverrides">The values that will be injected into the top level instance.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The instantiated object graph.</returns>
        /// <exception cref="ResolveTypeException">Thrown when the given type could not be resolved properly.</exception>
        public T Resolve<T>(DependencyOverrides dependencyOverrides, string registrationName = null)
        {
            return (T) PerformResolve(new TypeKey(typeof(T), registrationName),
                                      ResolveContext.CreateInitial(this, dependencyOverrides));
        }

        /// <summary>
        ///     Resolves the object graph for the given type, with an optional registration name. Overrides dependencies
        ///     with the specified <paramref name="dependencyOverrides" />.
        /// </summary>
        /// <param name="type">The type of the object graph root.</param>
        /// <param name="dependencyOverrides">The values that will be injected on the top level instance.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The instantiated object graph.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="ResolveTypeException">Thrown when the given type could not be resolved properly.</exception>
        public object Resolve(Type type, DependencyOverrides dependencyOverrides, string registrationName = null)
        {
            return PerformResolve(new TypeKey(type, registrationName),
                                  ResolveContext.CreateInitial(this, dependencyOverrides));
        }

        private object PerformResolve(TypeKey typeKey, ResolveContext initialContext)
        {
            var resolvedValue = ResolveRecursively(typeKey, initialContext);
            TryClearOverriddenMappings();
            return resolvedValue;
        }

        internal object ResolveRecursively(TypeKey typeKey, ResolveContext resolveContext)
        {
            object overriddenInstance;
            if (TryGetInstanceFromOverriddenMapping(typeKey, out overriddenInstance))
                return overriddenInstance;

            var registration = GetRegistration(typeKey);
            if (registration != null)
            {
                if (TryGetInstanceFromOverriddenMapping(registration.TypeKey, out overriddenInstance))
                    return overriddenInstance;
                return registration.Lifetime.GetInstance(CreationContext.FromCreationContext(resolveContext, registration));
            }

            if (typeKey == DiContainerType || typeKey == ServiceProviderType)
                return this;

            if (typeKey == GuidType)
                return Guid.NewGuid();

            registration = GetDefaultRegistration(typeKey);
            return registration.Lifetime.GetInstance(CreationContext.FromCreationContext(resolveContext, registration));
        }

        /// <summary>
        ///     Resolves all instances of the given abstraction type.
        /// </summary>
        /// <typeparam name="T">The abstraction type whose concrete instances should be resolved.</typeparam>
        /// <returns>An array containing the resolved instances.</returns>
        public T[] ResolveAll<T>()
        {
            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(typeof(T));
            var instances = new T[enumerator.GetNumberOfRegistrations()];
            var currentIndex = 0;
            var resolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            while (enumerator.MoveNext())
            {
                var registration = enumerator.Current;
                object instance;
                if (TryGetInstanceFromOverriddenMapping(registration.TypeKey, out instance) == false)
                    instance = registration.Lifetime.GetInstance(new CreationContext(this, registration, resolveScope));
                instances[currentIndex++] = (T) instance;
            }
            TryClearOverriddenMappings();

            return instances;
        }

        /// <summary>
        ///     Resolves all instances of the given abstraction type.
        /// </summary>
        /// <param name="type">The abstraction type whose concrete instances should be resolved.</param>
        /// <returns>An array containing the resolved instances.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public object[] ResolveAll(Type type)
        {
            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(type);
            var instances = new object[enumerator.GetNumberOfRegistrations()];
            var currentIndex = 0;
            var resolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            while (enumerator.MoveNext())
            {
                var registration = enumerator.Current;
                object instance;
                if (TryGetInstanceFromOverriddenMapping(registration.TypeKey, out instance) == false)
                    instance = registration.Lifetime.GetInstance(new CreationContext(this, registration, resolveScope));
                instances[currentIndex++] = instance;
            }
            TryClearOverriddenMappings();

            return instances;
        }

        private Registration GetDefaultRegistration(TypeKey typeKey)
        {
            typeKey.Type.MustBeAutomaticResolveCompliant();

            try
            {
                var registration = _typeMappings.GetOrAdd(typeKey,
                                                          () => Services.CreateDefaultRegistration(typeKey));
                return registration;
            }
            catch (TypeRegistrationException ex)
            {
                throw new ResolveTypeException($"A default registration for type {typeKey.GetFullRegistrationName()} could not be created - see the inner exception for details.", typeKey.Type, ex);
            }
        }

        /// <summary>
        ///     Creates a <see cref="DependencyOverrides" /> instance that can be used to override dependencies for the
        ///     top-level instance of the resolved object graph.
        /// </summary>
        /// <typeparam name="T">The type the <see cref="DependencyOverrides" /> should be created for.</typeparam>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        public DependencyOverrides OverrideDependenciesFor<T>(string registrationName = null)
        {
            return OverrideDependenciesFor(typeof(T), registrationName);
        }

        /// <summary>
        ///     Creates a <see cref="DependencyOverrides" /> instance that can be used to override dependencies for the
        ///     top-level instance of the resolved object graph.
        /// </summary>
        /// <param name="type">The type the <see cref="DependencyOverrides" /> should be created for.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the target registration has no TypeCreationInfo associated with it (e.g. when the registration describes an external instance).</exception>
        public DependencyOverrides OverrideDependenciesFor(Type type, string registrationName = null)
        {
            var typeKey = new TypeKey(type, registrationName);
            var targetRegistration = GetRegistration(typeKey) ?? GetDefaultRegistration(typeKey);
            EnsureTargetRegistrationHasTypeCreationInfo(targetRegistration);
            return new DependencyOverrides(targetRegistration.TypeCreationInfo);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void EnsureTargetRegistrationHasTypeCreationInfo(Registration registration)
        {
            if (registration.TypeCreationInfo == null)
                throw new InvalidOperationException($"ParameterOverrides cannot be created for type {registration.TypeKey.GetFullRegistrationName()} because there is no TypeCreationInfo associated with this registration.");
        }

        /// <summary>
        ///     Adds the specified instance to the <see cref="OverriddenMappings" /> which are injected
        ///     into the whole object graph where needed during the next Resolve / ResolveAll call.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The instance that will be injected during the next Resolve / ResolveAll call.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The container for method chaining.</returns>
        public DependencyInjectionContainer OverrideMapping<T>(T instance, string registrationName = null)
        {
            _overriddenMappings.Value.Add(new TypeKey(typeof(T), registrationName), instance);
            return this;
        }

        /// <summary>
        ///     Adds the specified instance to the <see cref="OverriddenMappings" /> which are injected
        ///     into the whole object graph where needed during the next Resolve / ResolveAll call.
        ///     The type of the instance will be used to find the target registration.
        /// </summary>
        /// <param name="instance">The instance that will be injected during the next Resolve / ResolveAll call.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance" /> is null.</exception>
        public DependencyInjectionContainer OverrideMapping(object instance, string registrationName = null)
        {
            instance.MustNotBeNull(nameof(instance));

            _overriddenMappings.Value.Add(new TypeKey(instance.GetType(), registrationName), instance);
            return this;
        }

        /// <summary>
        ///     Adds the specified instance to the <see cref="OverriddenMappings" /> which are injected
        ///     into the whole object graph where needed during the next Resolve / ResolveAll call.
        /// </summary>
        /// <param name="instance">The instance that will be injected during the next Resolve / ResolveAll call.</param>
        /// <param name="baseType">The type used to find the target registration.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The container for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance" /> or <paramref name="baseType" /> is null.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the type of <paramref name="instance" /> does not inherit from or implement <paramref name="baseType" />.</exception>
        public DependencyInjectionContainer OverrideMapping(object instance, Type baseType, string registrationName = null)
        {
            instance.MustNotBeNull(nameof(instance));
            instance.GetType().MustInheritFromOrImplement(baseType);

            _overriddenMappings.Value.Add(new TypeKey(baseType, registrationName), instance);
            return this;
        }

        private bool TryGetInstanceFromOverriddenMapping(TypeKey typeKey, out object instance)
        {
            if (_overriddenMappings.IsValueCreated)
                return _overriddenMappings.Value.TryGetValue(typeKey, out instance);

            instance = null;
            return false;
        }

        private void TryClearOverriddenMappings()
        {
            if (_overriddenMappings.IsValueCreated)
                _overriddenMappings.Value.Clear();
        }

        /// <summary>
        ///     Gets the registration for the specified type, using an optional registration name.
        /// </summary>
        /// <typeparam name="T">The type used to find the target registration.</typeparam>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The found registration, or null if none could be found.</returns>
        public Registration GetRegistration<T>(string registrationName = null)
        {
            return GetRegistration(new TypeKey(typeof(T), registrationName));
        }

        /// <summary>
        ///     Gets the registration for the specified type, using an optional registration name.
        /// </summary>
        /// <param name="type">The type used to find the target registration.</param>
        /// <param name="registrationName">The name of the target registration (optional).</param>
        /// <returns>The found registration, or null if none could be found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        public Registration GetRegistration(Type type, string registrationName = null)
        {
            return GetRegistration(new TypeKey(type, registrationName));
        }

        /// <summary>
        ///     Gets the registration for the specified type key.
        /// </summary>
        /// <param name="typeKey">The type key that identifies the target registration.</param>
        /// <returns>The found registration, or null if none could be found.</returns>
        public Registration GetRegistration(TypeKey typeKey)
        {
            Registration targetRegistration;
            if (_typeMappings.TryGetValue(typeKey, out targetRegistration))
                return targetRegistration;

            if (typeKey.Type.IsConstructedGenericType == false)
                return null;

            var genericTypeDefinition = typeKey.Type.GetGenericTypeDefinition();
            var genericTypeDefinitionKey = new TypeKey(genericTypeDefinition, typeKey.RegistrationName);
            Registration genericTypeDefinitionRegistration;
            if (_typeMappings.TryGetValue(genericTypeDefinitionKey, out genericTypeDefinitionRegistration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == genericTypeDefinitionRegistration.TargetType ? typeKey.Type : genericTypeDefinitionRegistration.TargetType.MakeGenericType(typeKey.Type.GenericTypeArguments);
            targetRegistration = _typeMappings.GetOrAdd(typeKey, () => genericTypeDefinitionRegistration.BindToClosedGenericType(closedConstructedType));
            return targetRegistration;
        }

        /// <summary>
        ///     Gets an enumerator that can be used to iterate over all registrations for the specified type.
        /// </summary>
        /// <param name="type">The type whose registrations should be retrieved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" />is null.</exception>
        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            return _typeMappings.GetRegistrationEnumeratorForType(type);
        }

        /// <summary>
        ///     Resolves all registrations that use the specified lifetime.
        /// </summary>
        /// <typeparam name="TLifetime">The lifetime which identifies the target registrations.</typeparam>
        /// <returns>The container for method chaining.</returns>
        public DependencyInjectionContainer InstantiateAllWithLifetime<TLifetime>() where TLifetime : Lifetime
        {
            var lifetimeType = typeof(TLifetime);
            var lazyResolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            foreach (var registration in _typeMappings.Registrations.Where(registration => registration.Lifetime.GetType() == lifetimeType))
            {
                registration.Lifetime.GetInstance(new CreationContext(this, registration, lazyResolveScope));
            }
            return this;
        }

        /// <summary>
        ///     Instantiates all registrations that are associated with a <see cref="SingletonLifetime" />.
        /// </summary>
        /// <returns>The container for method chaining.</returns>
        public DependencyInjectionContainer InstantiateAllSingletons()
        {
            return InstantiateAllWithLifetime<SingletonLifetime>();
        }

        /// <summary>
        ///     Instantiates all registrations that are associated with a <see cref="ScopedLifetime" />.
        /// </summary>
        /// <returns>The container for method chaining.</returns>
        public DependencyInjectionContainer InstantiateAllScopedObjects()
        {
            return InstantiateAllWithLifetime<ScopedLifetime>();
        }
    }
}