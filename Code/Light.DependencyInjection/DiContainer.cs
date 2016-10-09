using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer : IDisposable
    {
        private static readonly Type DiContainerType = typeof(DiContainer);
        private readonly RegistrationDictionary<GenericTypeDefinitionRegistration> _genericTypeDefinitonMappings;
        private readonly RegistrationDictionary<Registration> _typeMappings;
        public readonly ContainerScope Scope;
        private ContainerServices _containerServices;

        public DiContainer() : this(new ContainerServices()) { }

        public DiContainer(ContainerServices containerServices)
            : this(new RegistrationDictionary<Registration>(),
                   new RegistrationDictionary<GenericTypeDefinitionRegistration>(),
                   containerServices) { }

        private DiContainer(RegistrationDictionary<Registration> typeMappings,
                            RegistrationDictionary<GenericTypeDefinitionRegistration> genericTypeDefinitonMappings,
                            ContainerServices containerServices,
                            ContainerScope parentScope = null)
        {
            typeMappings.MustNotBeNull(nameof(typeMappings));
            genericTypeDefinitonMappings.MustNotBeNull(nameof(genericTypeDefinitonMappings));
            containerServices.MustNotBeNull(nameof(containerServices));

            _typeMappings = typeMappings;
            _genericTypeDefinitonMappings = genericTypeDefinitonMappings;
            _containerServices = containerServices;
            Scope = containerServices.ContainerScopeFactory.CreateScope(parentScope);
        }


        public IReadOnlyList<Registration> Registrations => _typeMappings.Registrations;

        public ContainerServices ContainerServices
        {
            get { return _containerServices; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _containerServices = value;
            }
        }


        public void Dispose()
        {
            Scope.Dispose();
        }

        public DiContainer CreateChildContainer(bool createEmptyChildScope = false, bool createCopyOfMappings = false)
        {
            var parentScope = createEmptyChildScope ? null : Scope;
            var registrations = createCopyOfMappings ? new RegistrationDictionary<Registration>(_typeMappings) : _typeMappings;
            var genericTypeDefinitionRegistrations = createCopyOfMappings ? new RegistrationDictionary<GenericTypeDefinitionRegistration>(_genericTypeDefinitonMappings) : _genericTypeDefinitonMappings;

            return new DiContainer(registrations,
                                   genericTypeDefinitionRegistrations,
                                   _containerServices,
                                   parentScope);
        }

        public DiContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _typeMappings.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
            }

            Register(registration);

            return this;
        }

        public DiContainer Register(Registration registration, params Type[] abstractionTypes)
        {
            return Register(registration, (IEnumerable<Type>) abstractionTypes);
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            _typeMappings.AddOrReplace(new TypeKey(registration.TargetType, registration.Name), registration);

            return this;
        }

        public DiContainer RegisterGenericTypeDefinition(GenericTypeDefinitionRegistration registration)
        {
            registration.MustNotBeNull(nameof(registration));

            _genericTypeDefinitonMappings.AddOrReplace(registration.TypeKey, registration);

            return this;
        }

        public DiContainer RegisterGenericTypeDefinition(GenericTypeDefinitionRegistration registration,
                                                         params Type[] abstractionTypes)
        {
            return RegisterGenericTypeDefinition(registration, (IEnumerable<Type>) abstractionTypes);
        }

        public DiContainer RegisterGenericTypeDefinition(GenericTypeDefinitionRegistration registration,
                                                         IEnumerable<Type> abstractionTypes)
        {
            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _genericTypeDefinitonMappings.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
            }

            RegisterGenericTypeDefinition(registration);
            return this;
        }

        public T Resolve<T>(string registrationName = null)
        {
            return (T) ResolveRecursively(new TypeKey(typeof(T), registrationName),
                                          CreationContext.CreateInitial(this));
        }

        public object Resolve(Type type, string registrationName = null)
        {
            return ResolveRecursively(new TypeKey(type, registrationName),
                                      CreationContext.CreateInitial(this));
        }

        public T Resolve<T>(ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return (T) ResolveRecursively(new TypeKey(typeof(T), registrationName),
                                          CreationContext.CreateInitial(this, parameterOverrides));
        }

        public object Resolve(Type type, ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return ResolveRecursively(new TypeKey(type, registrationName),
                                      CreationContext.CreateInitial(this, parameterOverrides));
        }

        internal object ResolveRecursively(TypeKey typeKey, CreationContext creationContext)
        {
            if (typeKey.Type == DiContainerType && typeKey.RegistrationName == null)
                return this;

            var registration = GetRegistration(typeKey) ?? GetDefaultRegistration(typeKey);
            return registration.Lifetime.GetInstance(new ResolveContext(this,
                                                                        registration,
                                                                        creationContext.LazyResolveScope,
                                                                        creationContext.ParameterOverrides));
        }

        private Registration GetDefaultRegistration(TypeKey typeKey)
        {
            CheckIfTypeIsInstantiable(typeKey.Type);

            var registration = _typeMappings.GetOrAdd(typeKey,
                                                      () => ContainerServices.CreateDefaultRegistration(typeKey));
            return registration;
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

        public Registration GetRegistration<T>(string registrationName = null)
        {
            return GetRegistration(typeof(T), registrationName);
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
            GenericTypeDefinitionRegistration genericTypeDefinitionRegistration;
            if (_genericTypeDefinitonMappings.TryGetValue(genericTypeDefinitionKey, out genericTypeDefinitionRegistration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == genericTypeDefinitionRegistration.TargetType ? typeKey.Type : genericTypeDefinitionRegistration.TargetType.MakeGenericType(typeKey.Type.GenericTypeArguments);
            targetRegistration = _typeMappings.GetOrAdd(typeKey, () => genericTypeDefinitionRegistration.BindToClosedGenericType(closedConstructedType));
            return targetRegistration;
        }

        public DiContainer InstantiateAllWithLifetime<TLifetime>() where TLifetime : ILifetime
        {
            var lifetimeType = typeof(TLifetime);
            var creationContext = CreationContext.CreateInitial(this);
            foreach (var registration in _typeMappings.Registrations.Where(registration => registration.Lifetime.GetType() == lifetimeType))
            {
                registration.Lifetime.GetInstance(new ResolveContext(this,
                                                                     registration,
                                                                     creationContext.LazyResolveScope));
            }
            return this;
        }

        public DiContainer InstantiateAllSingletons()
        {
            return InstantiateAllWithLifetime<SingletonLifetime>();
        }

        public DiContainer InstantiateAllScopedObjects()
        {
            return InstantiateAllWithLifetime<ScopedLifetime>();
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeIsInstantiable(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsAbstract)
                throw new ResolveTypeException($"The specified type \"{type}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.", type);

            if (typeInfo.IsEnum)
                throw new ResolveTypeException($"The specified type \"{type}\" describes an enum type which has not been registered and which cannot be resolved automatically.", type);

            if (typeInfo.BaseType == typeof(MulticastDelegate))
                throw new ResolveTypeException($"The specified type \"{type}\" describes a delegate type which has not been registered and which cannot be resolved automatically.", type);
        }

        public T[] ResolveAll<T>()
        {
            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(typeof(T));
            var instances = new T[enumerator.GetNumberOfRegistrations()];
            var creationContext = CreationContext.CreateInitial(this);
            var currentIndex = 0;
            while (enumerator.MoveNext())
            {
                instances[currentIndex++] = (T) enumerator.Current.Lifetime.GetInstance(new ResolveContext(this,
                                                                                                           enumerator.Current,
                                                                                                           creationContext.LazyResolveScope));
            }
            
            return instances;
        }
    }
}