using System;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class DiContainer : IDisposable
    {
        private readonly IConcurrentDictionary<Type, IConcurrentList<Registration>> _registrationMapping;
        private readonly IConcurrentDictionary<TypeKey, Func<object>> _resolveDelegates;
        private readonly ContainerServices _services;
        public readonly ContainerScope ContainerScope;

        public DiContainer(ContainerServices services = null)
        {
            _services = services ?? new ContainerServicesBuilder().Build();
            ContainerScope = _services.ContainerScopeFactory.CreateScope();
            _registrationMapping = _services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            _resolveDelegates = _services.ConcurrentDictionaryFactory.Create<TypeKey, Func<object>>();

            ContainerScope.TryAddDisposable(_registrationMapping);
            ContainerScope.TryAddDisposable(_resolveDelegates);
            _services.SetupContainer?.Invoke(this);
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

        public DiContainer RegisterInstance(object value, Action<IExternalInstanceOptions> configureRegistration = null)
        {
            value.MustNotBeNull(nameof(value));

            var registrationOptions = _services.CreateExternalInstanceOptions(value);
            configureRegistration?.Invoke(registrationOptions);

            return Register(registrationOptions.CreateRegistration());
        }

        private void AddMapping(Type type, Registration registration)
        {
            if (_registrationMapping.TryGetValue(type, out var typeRegistrations) == false)
            {
                typeRegistrations = _services.ConcurrentListFactory.Create<Registration>();
                typeRegistrations = _registrationMapping.GetOrAdd(type, typeRegistrations);
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

            if (_resolveDelegates.TryGetValue(typeKey, out var resolveDelegate))
                return resolveDelegate();

            resolveDelegate = _services.ResolveDelegateFactory.Create(typeKey, this);
            resolveDelegate = _resolveDelegates.GetOrAdd(typeKey, resolveDelegate);
            return resolveDelegate();
        }

        public Registration GetRegistration(TypeKey typeKey)
        {
            // TODO: check if generic type is asked
            if (_registrationMapping.TryGetValue(typeKey.Type, out var registrations) == false ||
                registrations.TryFindRegistration(typeKey, out var targetRegistration) == false)
                throw new ResolveException($"There is no registration for {typeKey}");

            return targetRegistration;
        }

        public void Dispose()
        {
            ContainerScope.Dispose();
        }
    }
}