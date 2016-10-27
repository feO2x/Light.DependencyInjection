using System;
using System.Collections.Generic;
using System.Linq;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class DependencyInjectionContainer : IServiceProvider, IDisposable
    {
        private static readonly Type DiContainerType = typeof(DependencyInjectionContainer);
        private static readonly Type ServiceProviderType = typeof(IServiceProvider);
        private static readonly Type GuidType = typeof(Guid);
        private readonly Dictionary<TypeKey, object> _overriddenMappings = new Dictionary<TypeKey, object>();

        private readonly RegistrationDictionary _typeMappings;
        public readonly ContainerScope Scope;
        private ContainerServices _services;

        public DependencyInjectionContainer() : this(new ContainerServices()) { }

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


        public IReadOnlyList<Registration> Registrations => _typeMappings.Registrations;
        public IReadOnlyDictionary<TypeKey, object> OverriddenMappings => _overriddenMappings;

        public ContainerServices Services
        {
            get { return _services; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _services = value;
            }
        }


        public void Dispose()
        {
            Scope.Dispose();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public DependencyInjectionContainer CreateChildContainer()
        {
            return CreateChildContainer(ChildContainerOptions.Default);
        }

        public DependencyInjectionContainer CreateChildContainer(ChildContainerOptions options)
        {
            var parentScope = options.CreateEmptyScope ? null : Scope;
            var registrations = options.CreateCopyOfMappings ? new RegistrationDictionary(_typeMappings) : _typeMappings;
            var services = options.CloneContainerServices ? _services.Clone() : _services;

            return new DependencyInjectionContainer(registrations, services, parentScope);
        }

        public DependencyInjectionContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _typeMappings.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
            }

            Register(registration);

            return this;
        }

        public DependencyInjectionContainer Register(Registration registration, params Type[] abstractionTypes)
        {
            return Register(registration, (IEnumerable<Type>) abstractionTypes);
        }

        public DependencyInjectionContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _typeMappings.AddOrReplace(new TypeKey(registration.TargetType, registration.Name), registration);

            return this;
        }

        public T Resolve<T>(string registrationName = null)
        {
            return (T) PerformResolve(new TypeKey(typeof(T), registrationName),
                                      CreationContext.CreateInitial(this));
        }

        public object Resolve(Type type, string registrationName = null)
        {
            return PerformResolve(new TypeKey(type, registrationName),
                                  CreationContext.CreateInitial(this));
        }

        public T Resolve<T>(ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return (T) PerformResolve(new TypeKey(typeof(T), registrationName),
                                      CreationContext.CreateInitial(this, parameterOverrides));
        }

        public object Resolve(Type type, ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return PerformResolve(new TypeKey(type, registrationName),
                                  CreationContext.CreateInitial(this, parameterOverrides));
        }

        private object PerformResolve(TypeKey typeKey, CreationContext initialContext)
        {
            var resolvedValue = ResolveRecursively(typeKey, initialContext);
            _overriddenMappings.Clear();
            return resolvedValue;
        }

        internal object ResolveRecursively(TypeKey typeKey, CreationContext creationContext)
        {
            object overriddenInstance;
            if (_overriddenMappings.TryGetValue(typeKey, out overriddenInstance))
                return overriddenInstance;

            var registration = GetRegistration(typeKey);
            if (registration != null)
            {
                if (_overriddenMappings.TryGetValue(registration.TypeKey, out overriddenInstance))
                    return overriddenInstance;
                return registration.Lifetime.GetInstance(ResolveContext.FromCreationContext(creationContext, registration));
            }

            if (typeKey == DiContainerType || typeKey == ServiceProviderType)
                return this;

            if (typeKey == GuidType)
                return Guid.NewGuid();

            registration = GetDefaultRegistration(typeKey);
            return registration.Lifetime.GetInstance(ResolveContext.FromCreationContext(creationContext, registration));
        }

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
                if (_overriddenMappings.TryGetValue(registration.TypeKey, out instance) == false)
                    instance = registration.Lifetime.GetInstance(new ResolveContext(this, registration, resolveScope));
                instances[currentIndex++] = (T) instance;
            }
            _overriddenMappings.Clear();

            return instances;
        }

        public object[] ResolveAll(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(type);
            var instances = new object[enumerator.GetNumberOfRegistrations()];
            var currentIndex = 0;
            var resolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            while (enumerator.MoveNext())
            {
                var registration = enumerator.Current;
                object instance;
                if (_overriddenMappings.TryGetValue(registration.TypeKey, out instance) == false)
                    instance = registration.Lifetime.GetInstance(new ResolveContext(this, registration, resolveScope));
                instances[currentIndex++] = instance;
            }
            _overriddenMappings.Clear();

            return instances;
        }

        private Registration GetDefaultRegistration(TypeKey typeKey)
        {
            typeKey.Type.MustBeResolveCompliant();

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

        public ParameterOverrides OverrideParametersFor<T>(string registrationName = null)
        {
            return OverrideParametersFor(typeof(T), registrationName);
        }

        public ParameterOverrides OverrideParametersFor(Type type, string registrationName = null)
        {
            var typeKey = new TypeKey(type, registrationName);
            var targetRegistration = GetRegistration(typeKey) ?? GetDefaultRegistration(typeKey);
            return new ParameterOverrides(targetRegistration.TypeCreationInfo);
        }

        public DependencyInjectionContainer OverrideMapping<T>(T instance, string registrationName = null)
        {
            _overriddenMappings.Add(new TypeKey(typeof(T), registrationName), instance);
            return this;
        }

        public DependencyInjectionContainer OverrideMapping(object instance, string registrationName = null)
        {
            instance.MustNotBeNull(nameof(instance));

            _overriddenMappings.Add(new TypeKey(instance.GetType(), registrationName), instance);
            return this;
        }

        public DependencyInjectionContainer OverrideMapping(object instance, Type baseType, string registrationName = null)
        {
            instance.MustNotBeNull(nameof(instance));
            instance.GetType().MustInheritFromOrImplement(baseType);

            _overriddenMappings.Add(new TypeKey(baseType, registrationName), instance);
            return this;
        }

        public Registration GetRegistration<T>(string registrationName = null)
        {
            return GetRegistration(new TypeKey(typeof(T), registrationName));
        }

        public Registration GetRegistration(Type type, string registrationName = null)
        {
            return GetRegistration(new TypeKey(type, registrationName));
        }

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

        public RegistrationEnumerator GetRegistrationEnumeratorForType(Type type)
        {
            return _typeMappings.GetRegistrationEnumeratorForType(type);
        }

        public DependencyInjectionContainer InstantiateAllWithLifetime<TLifetime>() where TLifetime : Lifetime
        {
            var lifetimeType = typeof(TLifetime);
            var lazyResolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            foreach (var registration in _typeMappings.Registrations.Where(registration => registration.Lifetime.GetType() == lifetimeType))
            {
                registration.Lifetime.GetInstance(new ResolveContext(this, registration, lazyResolveScope));
            }
            return this;
        }

        public DependencyInjectionContainer InstantiateAllSingletons()
        {
            return InstantiateAllWithLifetime<SingletonLifetime>();
        }

        public DependencyInjectionContainer InstantiateAllScopedObjects()
        {
            return InstantiateAllWithLifetime<ScopedLifetime>();
        }
    }
}