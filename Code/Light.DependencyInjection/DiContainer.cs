using System;
using System.Collections.Generic;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection
{

    public class DiContainer : IServiceProvider, IDisposable
    {
        private readonly IConcurrentDictionary<Type, IConcurrentList<Registration>> _registrationMappings;
        private readonly IConcurrentDictionary<ResolveDelegateId, ResolveDelegate> _resolveDelegates;
        public readonly ContainerScope Scope;
        public readonly ContainerServices Services;

        public DiContainer(ContainerServices services = null)
        {
            Services = services ?? new ContainerServicesBuilder().Build();
            Scope = Services.ContainerScopeFactory.CreateScope();
            _registrationMappings = Services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            _resolveDelegates = Services.ConcurrentDictionaryFactory.Create<ResolveDelegateId, ResolveDelegate>();

            Scope.TryAddDisposable(_registrationMappings);
            Scope.TryAddDisposable(_resolveDelegates);
            Services.SetupContainer?.Invoke(this);
        }

        private DiContainer(DiContainer parentContainer, ChildContainerOptions childContainerOptions)
        {
            Services = childContainerOptions.NewContainerServices ?? parentContainer.Services;
            _registrationMappings = childContainerOptions.DetachRegistrationMappingsFromParentContainer ? CreateCloneOfRegistrationMappings(parentContainer._registrationMappings) : parentContainer._registrationMappings;

            _resolveDelegates = childContainerOptions.DetachResolveDelegatesFromParentContainer ? Services.ConcurrentDictionaryFactory.Create<ResolveDelegateId, ResolveDelegate>() : parentContainer._resolveDelegates;
            Scope = Services.ContainerScopeFactory.CreateScope(parentContainer.Scope);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }

        private IConcurrentDictionary<Type, IConcurrentList<Registration>> CreateCloneOfRegistrationMappings(IConcurrentDictionary<Type, IConcurrentList<Registration>> parentRegistrationMappings)
        {
            var newRegistrationMappings = Services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            foreach (var registrationMapping in parentRegistrationMappings)
            {
                newRegistrationMappings.TryAdd(registrationMapping.Key, Services.RegistrationCollectionFactory.Create(registrationMapping.Value));
            }
            return newRegistrationMappings;
        }

        public DiContainer Register<T>(Action<IRegistrationOptions<T>> configureRegistration = null)
        {
            var registrationOptions = Services.CreateRegistrationOptions<T>();
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register(Type targetType, Action<IRegistrationOptions> configureRegistration = null)
        {
            var registrationOptions = Services.CreateRegistrationOptions(targetType);
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register<TAbstract, TConcrete>(Action<IRegistrationOptions<TConcrete>> configureRegistration = null) where TConcrete : TAbstract
        {
            var registrationOptions = Services.CreateRegistrationOptions<TConcrete>();
            registrationOptions.MapToAbstractions(typeof(TAbstract));
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register(Type abstractionType, Type concreteType, Action<IRegistrationOptions> configureRegistration = null)
        {
            var registrationOptions = Services.CreateRegistrationOptions(concreteType);
            registrationOptions.MapToAbstractions(abstractionType);
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            AddMapping(registration.TargetType, registration);

            if (registration.MappedAbstractionTypes.IsNullOrEmpty()) return this;

            for (var i = 0; i < registration.MappedAbstractionTypes.Count; i++)
            {
                AddMapping(registration.MappedAbstractionTypes[i], registration);
            }
            return this;
        }

        public DiContainer Register(object value, Action<IExternalInstanceOptions> configureRegistration = null)
        {
            value.MustNotBeNull(nameof(value));

            var registrationOptions = Services.CreateExternalInstanceOptions(value);
            configureRegistration?.Invoke(registrationOptions);

            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer RegisterMany<TAbstraction>(IEnumerable<Type> concreteTypes, Action<IRegistrationOptions> configureRegistrations = null)
        {
            return RegisterMany(typeof(TAbstraction), concreteTypes, configureRegistrations);
        }

        public DiContainer RegisterMany(Type abstractionType, IEnumerable<Type> concreteTypes, Action<IRegistrationOptions> configureRegistrations = null)
        {
            abstractionType.MustNotBeNull(nameof(abstractionType));
            var concreteTypesList = concreteTypes.AsReadOnlyList();
            concreteTypesList.MustNotBeNullOrEmpty(nameof(concreteTypes));

            for (var i = 0; i < concreteTypesList.Count; i++)
            {
                var registrationOptions = Services.CreateRegistrationOptions(concreteTypesList[i]);
                registrationOptions.MapToAbstractions(abstractionType).UseTypeNameAsRegistrationName();
                configureRegistrations?.Invoke(registrationOptions);
                Register(registrationOptions.CreateRegistration());
            }

            return this;
        }

        public DiContainer PrepareScopedExternalInstance<T>(Action<IExternalInstanceOptions> configureRegistration = null)
        {
            return PrepareScopedExternalInstance(typeof(T), configureRegistration);
        }

        public DiContainer PrepareScopedExternalInstance(Type type, Action<IExternalInstanceOptions> configureRegistration = null)
        {
            var registrationOptions = Services.CreateScopedExternalInstanceOptions(type);
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer PrepareScopedExternalInstance<TAbstract, TConcrete>(Action<IExternalInstanceOptions> configureRegistration = null) where TConcrete : TAbstract
        {
            return PrepareScopedExternalInstance(typeof(TAbstract), typeof(TConcrete), configureRegistration);
        }

        public DiContainer PrepareScopedExternalInstance(Type abstractionType, Type concreteType, Action<IExternalInstanceOptions> configureRegistration = null)
        {
            var registrationOptions = Services.CreateScopedExternalInstanceOptions(concreteType);
            registrationOptions.MapToAbstractions(abstractionType);
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer AddPreparedExternalInstanceToScope<T>(T instance, string registrationName = "")
        {
            return AddPreparedExternalInstanceToScope(instance, typeof(T), registrationName);
        }

        public DiContainer AddPreparedExternalInstanceToScope(object instance, Type targetType, string registrationName = "")
        {
            instance.MustNotBeNull(nameof(instance));

            var registration = TryGetRegistration(new TypeKey(targetType, registrationName));
            if (registration == null || registration.Lifetime is ScopedExternalInstanceLifetime == false)
                throw new RegistrationException($"There is no registration present for the Scoped External Instance \"{instance}\".");

            Scope.AddOrUpdateScopedInstance(registration.TypeKey, instance);
            if (registration.IsTrackingDisposables)
                Scope.TryAddDisposable(instance);

            return this;
        }

        private void AddMapping(Type type, Registration registration)
        {
            if (_registrationMappings.TryGetValue(type, out var typeRegistrations) == false)
            {
                typeRegistrations = Services.RegistrationCollectionFactory.Create();
                typeRegistrations = _registrationMappings.GetOrAdd(type, typeRegistrations);
            }
            typeRegistrations.AddOrUpdate(registration);
        }

        public T Resolve<T>(string registrationName = "")
        {
            return (T) Resolve(new TypeKey(typeof(T), registrationName), null);
        }

        public object Resolve(Type type, string registrationName = "")
        {
            return Resolve(new TypeKey(type, registrationName), null);
        }

        public object Resolve(TypeKey typeKey, IDependencyOverrideOptions dependencyOverrideOptions)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));

            var dependencyOverrides = dependencyOverrideOptions?.MustBeOfType<DependencyOverrideOptions>(nameof(dependencyOverrideOptions)).Build();
            var resolveDelegateId = new ResolveDelegateId(typeKey, dependencyOverrides);
            if (_resolveDelegates.TryGetValue(resolveDelegateId, out var resolveDelegate) == false)
            {
                var dependencyOverridesKey = dependencyOverrides?.CreateDependencyOverridesKey();
                resolveDelegate = Services.ResolveDelegateFactory.Create(typeKey, dependencyOverridesKey, this);
                resolveDelegate = _resolveDelegates.GetOrAdd(new ResolveDelegateId(typeKey, dependencyOverridesKey), resolveDelegate);
            }
            return resolveDelegate(Services.ResolveContextFactory.Create(this, dependencyOverrides));
        }

        public T Resolve<T>(IDependencyOverrideOptions dependencyOverrides, string registrationName = "")
        {
            return (T) Resolve(new TypeKey(typeof(T), registrationName), dependencyOverrides);
        }

        public object Resolve(Type type, IDependencyOverrideOptions dependencyOverrideOptions, string registrationName = "")
        {
            return Resolve(new TypeKey(type, registrationName), dependencyOverrideOptions);
        }

        public IList<T> ResolveAll<T>()
        {
            return ResolveAll<T>(typeof(T));
        }

        public IList<object> ResolveAll(Type type)
        {
            return ResolveAll<object>(type);
        }

        private T[] ResolveAll<T>(Type type)
        {
            if (_registrationMappings.TryGetValue(type, out var registrations) == false)
                throw new ResolveException($"There are no types registered with the DI Container that map to type \"{type}\".");

            var array = new T[registrations.Count];
            for (var i = 0; i < registrations.Count; i++)
            {
                array[i] = (T) Resolve(registrations[i]);
            }
            return array;
        }

        public T Resolve<T>(Registration registration, IDependencyOverrideOptions dependencyOverrideOptions = null)
        {
            return (T) Resolve(registration, dependencyOverrideOptions);
        }

        public object Resolve(Registration registration, IDependencyOverrideOptions dependencyOverrideOptions = null)
        {
            registration.MustNotBeNull(nameof(registration));

            var dependencyOverrides = dependencyOverrideOptions?.MustBeOfType<DependencyOverrideOptions>(nameof(dependencyOverrideOptions)).Build();
            var resolveDelegateId = new ResolveDelegateId(registration.TypeKey);
            if (_resolveDelegates.TryGetValue(resolveDelegateId, out var resolveDelegate) == false)
            {
                var dependencyOverridesKey = dependencyOverrides?.CreateDependencyOverridesKey();
                resolveDelegate = Services.ResolveDelegateFactory.Create(registration, dependencyOverridesKey, this);
                resolveDelegate = _resolveDelegates.GetOrAdd(new ResolveDelegateId(registration.TypeKey, dependencyOverridesKey), resolveDelegate);
            }

            return resolveDelegate(Services.ResolveContextFactory.Create(this));
        }

        public Registration GetRegistration<T>(string registrationName = "")
        {
            return GetRegistration(new TypeKey(typeof(T), registrationName));
        }

        public Registration GetRegistration(Type type, string registrationName = "")
        {
            return GetRegistration(new TypeKey(type, registrationName));
        }

        public Registration GetRegistration(TypeKey typeKey)
        {
            var targetRegistration = TryGetRegistration(typeKey);
            if (targetRegistration != null)
                return targetRegistration;

            targetRegistration = Services.AutomaticRegistrationFactory.CreateDefaultRegistration(typeKey, this);
            if (targetRegistration != null)
                Register(targetRegistration);

            return targetRegistration;
        }

        public Registration TryGetRegistration<T>(string registrationName = "")
        {
            return TryGetRegistration(new TypeKey(typeof(T), registrationName));
        }

        public Registration TryGetRegistration(Type type, string registrationName = "")
        {
            return TryGetRegistration(new TypeKey(type, registrationName));
        }

        public Registration TryGetRegistration(TypeKey typeKey)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));

            if (_registrationMappings.TryGetValue(typeKey.Type, out var registrations) && registrations.TryFindRegistration(typeKey, out var targetRegistration))
                return targetRegistration;

            if (typeKey.Type.IsClosedConstructedGenericType() == false)
                return null;

            var genericTypeDefinition = typeKey.Type.GetGenericTypeDefinition();
            if (_registrationMappings.TryGetValue(genericTypeDefinition, out registrations) && registrations.TryFindRegistration(new TypeKey(genericTypeDefinition, typeKey.RegistrationName), out targetRegistration))
                return targetRegistration;

            return null;
        }

        public IReadOnlyList<Registration> GetRegistrationsForType<T>()
        {
            return GetRegistrationsForType(typeof(T));
        }

        public IReadOnlyList<Registration> GetRegistrationsForType(Type type)
        {
            type.MustNotBeNull(nameof(type));
            return _registrationMappings.TryGetValue(type, out var registrations) ? registrations.AsReadOnlyList() : null;
        }

        public DiContainer CreateChildContainer(ChildContainerOptions childContainerOptions = default(ChildContainerOptions))
        {
            return new DiContainer(this, childContainerOptions);
        }

        public ResolveInfo GetResolveInfo(Type type, string registrationName = "", bool? tryResolveAll = null)
        {
            return GetResolveInfo(new TypeKey(type, registrationName), tryResolveAll);
        }

        public ResolveInfo GetResolveInfo(TypeKey typeKey, bool? tryResolveAll = null)
        {
            return Services.ResolveInfoAlgorithm.Search(typeKey, this, tryResolveAll);
        }

        public IDependencyOverrideOptions OverrideDependenciesFor<T>(string registrationName = "")
        {
            return OverrideDependenciesFor(typeof(T), registrationName);
        }

        public IDependencyOverrideOptions OverrideDependenciesFor(Type type, string registrationName = "")
        {
            var typeKey = new TypeKey(type, registrationName);
            var targetRegistration = GetRegistration(typeKey);
            if (targetRegistration == null)
                throw new ResolveException($"You cannot override the dependencies for {typeKey} because the DI Container has no registration for this type.");

            return new DependencyOverrideOptions(targetRegistration);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }
    }
}