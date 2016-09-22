using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer : IDisposable
    {
        private static readonly Type DiContainerType = typeof(DiContainer);
        private readonly FastReadThreadSafeDictionary<TypeKey, Registration> _registrations;
        public readonly ContainerScope Scope;
        private IDefaultRegistrationFactory _defaultRegistrationFactory;
        private IInjectorForUnknownInstanceMembers _injectorForUnknownInstanceMembers;
        private TypeAnalyzer _typeAnalyzer;

        public DiContainer() : this(new FastReadThreadSafeDictionary<TypeKey, Registration>()) { }

        public DiContainer(FastReadThreadSafeDictionary<TypeKey, Registration> registrations)
            : this(registrations,
                   new TransientRegistrationFactory(),
                   new DefaultInjectorForUnknownInstanceMembers(),
                   new TypeAnalyzer(),
                   new ContainerScope()) { }

        private DiContainer(FastReadThreadSafeDictionary<TypeKey, Registration> registrations,
                            IDefaultRegistrationFactory defaultRegistrationFactory,
                            IInjectorForUnknownInstanceMembers injectorForUnknownInstanceMembers,
                            TypeAnalyzer typeAnalyzer,
                            ContainerScope scope)
        {
            registrations.MustNotBeNull(nameof(registrations));

            _registrations = registrations;
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

            return new DiContainer(registrations,
                                   _defaultRegistrationFactory,
                                   _injectorForUnknownInstanceMembers,
                                   _typeAnalyzer,
                                   childScope);
        }

        public DiContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            Register(registration);

            foreach (var abstractionType in abstractionTypes)
            {
                registration.TargetType.MustInheritFromOrImplement(abstractionType);
                _registrations.AddOrReplace(new TypeKey(abstractionType, registration.Name), registration);
            }

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

        public T Resolve<T>(string registrationName = null)
        {
            return (T) Resolve(typeof(T), registrationName);
        }

        public object Resolve(Type type, string registrationName = null)
        {
            if (type == DiContainerType && registrationName == null)
                return this;

            object singleton;
            if (Scope.TryGetObject(new TypeKey(type, registrationName), out singleton))
                return singleton;

            var registration = GetRegistration(type, registrationName) ?? TryCreateDefaultRegistration(type, registrationName);

            var instance = registration.GetInstance(this);
            if (registration.IsContainerTrackingDisposable)
                Scope.TryAddDisposable(instance);
            return instance;
        }

        public T Resolve<T>(ParameterOverrides parameterOverrides, string registrationName = null)
        {
            return (T) Resolve(typeof(T), parameterOverrides, registrationName);
        }

        public object Resolve(Type type, ParameterOverrides parameterOverrides, string registrationName = null)
        {
            if (type == DiContainerType && registrationName == null)
                return this;

            var registration = GetRegistration(type, registrationName) ?? TryCreateDefaultRegistration(type, registrationName);
            var instance = registration.CreateInstance(this, parameterOverrides);
            Scope.TryAddDisposable(instance);
            return instance;
        }

        private Registration TryCreateDefaultRegistration(Type type, string registrationName)
        {
            CheckIfTypeIsInstantiable(type);

            var registration = _registrations.GetOrAdd(new TypeKey(type, registrationName),
                                                       () => _defaultRegistrationFactory.CreateDefaultRegistration(_typeAnalyzer.CreateInfoFor(type)));
            return registration;
        }

        public ParameterOverrides OverrideParametersFor<T>(string registrationName = null)
        {
            return OverrideParametersFor(typeof(T), registrationName);
        }

        public ParameterOverrides OverrideParametersFor(Type type, string registrationName = null)
        {
            var targetRegistration = GetRegistration(type, registrationName) ?? TryCreateDefaultRegistration(type, registrationName);
            CheckRegistrationForParameterOverrides(targetRegistration);

            return new ParameterOverrides(targetRegistration.TypeCreationInfo);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        // ReSharper disable once UnusedParameter.Local
        private static void CheckRegistrationForParameterOverrides(Registration targetRegistration)
        {
            if (targetRegistration.IsCreatingNewInstanceOnNextResolve == false)
                throw new InvalidOperationException($"The type {targetRegistration.TypeKey.GetFullRegistrationName()} will not be instantiated on the next resolve call, but use an existing instance. Thus it's parameters cannot be overridden.");
            if (targetRegistration.TypeCreationInfo == null)
                throw new InvalidOperationException($"The type {targetRegistration.TypeKey.GetFullRegistrationName()} is not instantiated by the DI container and thus cannot have it's parameters overridden.");
        }

        public Registration GetRegistration<T>(string registrationName = null)
        {
            return GetRegistration(typeof(T), registrationName);
        }

        public Registration GetRegistration(Type type, string registrationName = null)
        {
            var typeKey = new TypeKey(type, registrationName);
            Registration returnedRegistration;
            if (_registrations.TryGetValue(typeKey, out returnedRegistration))
                return returnedRegistration;

            if (type.IsConstructedGenericType == false)
                return null;

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericTypeDefinitionKey = new TypeKey(genericTypeDefinition, registrationName);
            Registration genericTypeDefinitionRegistration;
            if (_registrations.TryGetValue(genericTypeDefinitionKey, out genericTypeDefinitionRegistration) == false)
                return null;

            var closedConstructedType = genericTypeDefinition == genericTypeDefinitionRegistration.TargetType ? type : genericTypeDefinitionRegistration.TargetType.MakeGenericType(type.GenericTypeArguments);
            returnedRegistration = _registrations.GetOrAdd(typeKey, () => genericTypeDefinitionRegistration.BindGenericTypeDefinition(closedConstructedType));
            return returnedRegistration;
        }

        public DiContainer ResolveAllSingletons()
        {
            foreach (var singletonRegistration in _registrations.Values.OfType<ISingletonRegistration>())
            {
                var instance = singletonRegistration.GetInstance(this);
                if (singletonRegistration.IsContainerTrackingDisposable)
                    Scope.TryAddDisposable(instance);
            }
            return this;
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