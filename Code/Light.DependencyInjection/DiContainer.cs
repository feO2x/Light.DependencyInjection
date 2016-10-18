﻿using System;
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
        private ContainerServices _services;

        public DiContainer() : this(new ContainerServices()) { }

        public DiContainer(ContainerServices containerServices)
            : this(new RegistrationDictionary<Registration>(),
                   new RegistrationDictionary<GenericTypeDefinitionRegistration>(),
                   containerServices) { }

        private DiContainer(RegistrationDictionary<Registration> typeMappings,
                            RegistrationDictionary<GenericTypeDefinitionRegistration> genericTypeDefinitonMappings,
                            ContainerServices services,
                            ContainerScope parentScope = null)
        {
            typeMappings.MustNotBeNull(nameof(typeMappings));
            genericTypeDefinitonMappings.MustNotBeNull(nameof(genericTypeDefinitonMappings));
            services.MustNotBeNull(nameof(services));

            _typeMappings = typeMappings;
            _genericTypeDefinitonMappings = genericTypeDefinitonMappings;
            _services = services;
            Scope = services.ContainerScopeFactory.CreateScope(parentScope);
        }


        public IReadOnlyList<Registration> Registrations => _typeMappings.Registrations;

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

        public DiContainer CreateChildContainer()
        {
            return CreateChildContainer(ChildContainerOptions.Default);
        }

        public DiContainer CreateChildContainer(ChildContainerOptions options)
        {
            var parentScope = options.CreateEmptyScope ? null : Scope;
            var registrations = options.CreateCopyOfMappings ? new RegistrationDictionary<Registration>(_typeMappings) : _typeMappings;
            var genericTypeDefinitionRegistrations = options.CreateCopyOfMappings ? new RegistrationDictionary<GenericTypeDefinitionRegistration>(_genericTypeDefinitonMappings) : _genericTypeDefinitonMappings;

            return new DiContainer(registrations,
                                   genericTypeDefinitionRegistrations,
                                   options.CloneContainerServices ? _services.Clone() : _services,
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
            return registration.Lifetime.GetInstance(ResolveContext.FromCreationContext(creationContext, registration));
        }

        private Registration GetDefaultRegistration(TypeKey typeKey)
        {
            CheckIfTypeIsInstantiable(typeKey.Type);

            var registration = _typeMappings.GetOrAdd(typeKey,
                                                      () => Services.CreateDefaultRegistration(typeKey));
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
            GenericTypeDefinitionRegistration genericTypeDefinitionRegistration;
            if (_genericTypeDefinitonMappings.TryGetValue(genericTypeDefinitionKey, out genericTypeDefinitionRegistration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == genericTypeDefinitionRegistration.TargetType ? typeKey.Type : genericTypeDefinitionRegistration.TargetType.MakeGenericType(typeKey.Type.GenericTypeArguments);
            targetRegistration = _typeMappings.GetOrAdd(typeKey, () => genericTypeDefinitionRegistration.BindToClosedGenericType(closedConstructedType));
            return targetRegistration;
        }

        public RegistrationEnumerator<Registration> GetRegistrationEnumeratorForType(Type type)
        {
            return _typeMappings.GetRegistrationEnumeratorForType(type);
        }

        public DiContainer InstantiateAllWithLifetime<TLifetime>() where TLifetime : ILifetime
        {
            var lifetimeType = typeof(TLifetime);
            var lazyResolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            foreach (var registration in _typeMappings.Registrations.Where(registration => registration.Lifetime.GetType() == lifetimeType))
            {
                registration.Lifetime.GetInstance(new ResolveContext(this, registration, lazyResolveScope));
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

        public T[] ResolveAll<T>() // TODO: with ParameterOverrides?
        {
            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(typeof(T));
            var instances = new T[enumerator.GetNumberOfRegistrations()];
            var currentIndex = 0;
            var resolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            while (enumerator.MoveNext())
            {
                instances[currentIndex++] = (T) enumerator.Current.Lifetime.GetInstance(new ResolveContext(this, enumerator.Current, resolveScope));
            }

            return instances;
        }

        public object[] ResolveAll(Type type) // TODO: with ParameterOverrides?
        {
            type.MustNotBeNull(nameof(type));

            var enumerator = _typeMappings.GetRegistrationEnumeratorForType(type);
            var instances = new object[enumerator.GetNumberOfRegistrations()];
            var currentIndex = 0;
            var resolveScope = Services.ResolveScopeFactory.CreateLazyScope();
            while (enumerator.MoveNext())
            {
                instances[currentIndex++] = enumerator.Current.Lifetime.GetInstance(new ResolveContext(this, enumerator.Current, resolveScope));
            }

            return instances;
        }
    }
}