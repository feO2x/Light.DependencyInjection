using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer : IDisposable
    {
        private static readonly Type DiContainerType = typeof(DiContainer);
        private readonly FastReadThreadSafeDictionary<TypeKey, GenericTypeDefinitionRegistration> _genericTypeDefinitionRegistrations;
        private readonly FastReadThreadSafeDictionary<TypeKey, Registration> _registrations;
        public readonly ContainerScope Scope;
        private IDefaultRegistrationFactory _defaultRegistrationFactory;
        private IInjectorForUnknownInstanceMembers _injectorForUnknownInstanceMembers;
        private TypeAnalyzer _typeAnalyzer;

        public DiContainer() : this(new FastReadThreadSafeDictionary<TypeKey, Registration>(),
                                    new FastReadThreadSafeDictionary<TypeKey, GenericTypeDefinitionRegistration>()) { }

        public DiContainer(FastReadThreadSafeDictionary<TypeKey, Registration> registrations,
                           FastReadThreadSafeDictionary<TypeKey, GenericTypeDefinitionRegistration> genericTypeDefinitionRegistrations)
            : this(registrations,
                   genericTypeDefinitionRegistrations,
                   new TransientRegistrationFactory(),
                   new DefaultInjectorForUnknownInstanceMembers(),
                   new TypeAnalyzer(),
                   new ContainerScope()) { }

        private DiContainer(FastReadThreadSafeDictionary<TypeKey, Registration> registrations,
                            FastReadThreadSafeDictionary<TypeKey, GenericTypeDefinitionRegistration> genericTypeDefinitionRegistrations,
                            IDefaultRegistrationFactory defaultRegistrationFactory,
                            IInjectorForUnknownInstanceMembers injectorForUnknownInstanceMembers,
                            TypeAnalyzer typeAnalyzer,
                            ContainerScope scope)
        {
            registrations.MustNotBeNull(nameof(registrations));
            genericTypeDefinitionRegistrations.MustNotBeNull(nameof(genericTypeDefinitionRegistrations));

            _registrations = registrations;
            _genericTypeDefinitionRegistrations = genericTypeDefinitionRegistrations;
            _defaultRegistrationFactory = defaultRegistrationFactory;
            _injectorForUnknownInstanceMembers = injectorForUnknownInstanceMembers;
            _typeAnalyzer = typeAnalyzer;
            Scope = scope;
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

        public IReadOnlyList<Registration> Registrations => _registrations.Values;

        public TypeAnalyzer TypeAnalyzer
        {
            get { return _typeAnalyzer; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _typeAnalyzer = value;
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

        public void Dispose()
        {
            Scope.Dispose();
        }

        public DiContainer CreateChildContainer(bool createEmptyChildScope = false, bool createCopyOfMappings = false)
        {
            var childScope = createEmptyChildScope ? new ContainerScope() : new ContainerScope(Scope);
            var registrations = createCopyOfMappings ? new FastReadThreadSafeDictionary<TypeKey, Registration>(_registrations) : _registrations;
            var genericTypeDefinitionRegistrations = createCopyOfMappings ? new FastReadThreadSafeDictionary<TypeKey, GenericTypeDefinitionRegistration>(_genericTypeDefinitionRegistrations) : _genericTypeDefinitionRegistrations;

            return new DiContainer(registrations,
                                   genericTypeDefinitionRegistrations,
                                   _defaultRegistrationFactory,
                                   _injectorForUnknownInstanceMembers,
                                   _typeAnalyzer,
                                   childScope);
        }

        public DiContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _registrations.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
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

            _registrations.AddOrReplace(new TypeKey(registration.TargetType, registration.Name), registration);

            return this;
        }

        public DiContainer RegisterGenericTypeDefinition(GenericTypeDefinitionRegistration registration)
        {
            registration.MustNotBeNull(nameof(registration));

            _genericTypeDefinitionRegistrations.AddOrReplace(registration.TypeKey, registration);

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
                _genericTypeDefinitionRegistrations.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
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

            var registration = _registrations.GetOrAdd(typeKey,
                                                       () => _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(typeKey.Type)));
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
            if (_registrations.TryGetValue(typeKey, out targetRegistration))
                return targetRegistration;

            if (typeKey.Type.IsConstructedGenericType == false)
                return null;

            var genericTypeDefinition = typeKey.Type.GetGenericTypeDefinition();
            var genericTypeDefinitionKey = new TypeKey(genericTypeDefinition, typeKey.RegistrationName);
            GenericTypeDefinitionRegistration genericTypeDefinitionRegistration;
            if (_genericTypeDefinitionRegistrations.TryGetValue(genericTypeDefinitionKey, out genericTypeDefinitionRegistration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == genericTypeDefinitionRegistration.TargetType ? typeKey.Type : genericTypeDefinitionRegistration.TargetType.MakeGenericType(typeKey.Type.GenericTypeArguments);
            targetRegistration = _registrations.GetOrAdd(typeKey, () => genericTypeDefinitionRegistration.BindToClosedGenericType(closedConstructedType));
            return targetRegistration;
        }

        public DiContainer InstantiateAllWithLifetime<TLifetime>() where TLifetime : ILifetime
        {
            var lifetimeType = typeof(TLifetime);
            foreach (var registration in _registrations.Values.Where(registration => registration.Lifetime.GetType() == lifetimeType))
            {
                registration.Lifetime.GetInstance(new ResolveContext(this, registration, null));
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
    }
}