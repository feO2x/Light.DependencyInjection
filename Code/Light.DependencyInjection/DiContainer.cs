using System;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeResolving;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class DiContainer : IDisposable
    {
        private readonly IConcurrentDictionary<Type, IConcurrentList<Registration>> _registrationMappings;
        private readonly IConcurrentDictionary<TypeKey, ResolveDelegate> _resolveDelegates;
        private readonly ContainerServices _services;
        public readonly ContainerScope Scope;

        public DiContainer(ContainerServices services = null)
        {
            _services = services ?? new ContainerServicesBuilder().Build();
            Scope = _services.ContainerScopeFactory.CreateScope();
            _registrationMappings = _services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            _resolveDelegates = _services.ConcurrentDictionaryFactory.Create<TypeKey, ResolveDelegate>();

            Scope.TryAddDisposable(_registrationMappings);
            Scope.TryAddDisposable(_resolveDelegates);
            _services.SetupContainer?.Invoke(this);
        }

        private DiContainer(DiContainer parentContainer)
        {
            _registrationMappings = parentContainer._registrationMappings;
            _resolveDelegates = parentContainer._resolveDelegates;
            _services = parentContainer._services;
            Scope = _services.ContainerScopeFactory.CreateScope(parentContainer.Scope);
        }

        public ContainerServices Services => _services;

        public DiContainer Register<T>(Action<IRegistrationOptions<T>> configureRegistration = null)
        {
            var registrationOptions = _services.CreateRegistrationOptions<T>();
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register(Type targetType, Action<IRegistrationOptions> configureRegistration = null)
        {
            var registrationOptions = _services.CreateRegistrationOptions(targetType);
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register<TAbstract, TConcrete>(Action<IRegistrationOptions<TConcrete>> configureRegistration = null) where TConcrete : TAbstract
        {
            var registrationOptions = _services.CreateRegistrationOptions<TConcrete>();
            registrationOptions.MapToAbstractions(typeof(TAbstract));
            configureRegistration?.Invoke(registrationOptions);
            return Register(registrationOptions.CreateRegistration());
        }

        public DiContainer Register(Type abstractionType, Type concreteType, Action<IRegistrationOptions> configureRegistration = null)
        {
            var registrationOptions = _services.CreateRegistrationOptions(concreteType);
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

            var registrationOptions = _services.CreateExternalInstanceOptions(value);
            configureRegistration?.Invoke(registrationOptions);

            return Register(registrationOptions.CreateRegistration());
        }

        private void AddMapping(Type type, Registration registration)
        {
            if (_registrationMappings.TryGetValue(type, out var typeRegistrations) == false)
            {
                typeRegistrations = _services.ConcurrentListFactory.Create<Registration>();
                typeRegistrations = _registrationMappings.GetOrAdd(type, typeRegistrations);
            }
            typeRegistrations.AddOrUpdate(registration);
        }

        public T Resolve<T>(string registrationName = "")
        {
            return (T) Resolve(new TypeKey(typeof(T), registrationName));
        }

        public object Resolve(Type type, string registrationName = "")
        {
            return Resolve(new TypeKey(type, registrationName));
        }

        private object Resolve(TypeKey typeKey)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey));

            if (_resolveDelegates.TryGetValue(typeKey, out var resolveDelegate) == false)
            {
                resolveDelegate = _services.ResolveDelegateFactory.Create(typeKey, this);
                resolveDelegate = _resolveDelegates.GetOrAdd(typeKey, resolveDelegate);
            }
            return resolveDelegate(new ResolveContext(this));
        }

        public Registration GetRegistration(TypeKey typeKey)
        {
            // TODO: check if generic type is asked
            if (_registrationMappings.TryGetValue(typeKey.Type, out var registrations) && registrations.TryFindRegistration(typeKey, out var targetRegistration))
                return targetRegistration;

            targetRegistration = _services.AutomaticRegistrationFactory.CreateDefaultRegistration(typeKey, this);
            if (targetRegistration != null)
                Register(targetRegistration);

            return targetRegistration;
        }

        public DiContainer CreateChildContainer()
        {
            return new DiContainer(this);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}