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
        public readonly ContainerServices Services;
        public readonly ContainerScope Scope;

        public DiContainer(ContainerServices services = null)
        {
            Services = services ?? new ContainerServicesBuilder().Build();
            Scope = Services.ContainerScopeFactory.CreateScope();
            _registrationMappings = Services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            _resolveDelegates = Services.ConcurrentDictionaryFactory.Create<TypeKey, ResolveDelegate>();

            Scope.TryAddDisposable(_registrationMappings);
            Scope.TryAddDisposable(_resolveDelegates);
            Services.SetupContainer?.Invoke(this);
        }

        private DiContainer(DiContainer parentContainer)
        {
            _registrationMappings = parentContainer._registrationMappings;
            _resolveDelegates = parentContainer._resolveDelegates;
            Services = parentContainer.Services;
            Scope = Services.ContainerScopeFactory.CreateScope(parentContainer.Scope);
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

        private void AddMapping(Type type, Registration registration)
        {
            if (_registrationMappings.TryGetValue(type, out var typeRegistrations) == false)
            {
                typeRegistrations = Services.ConcurrentListFactory.Create<Registration>();
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
                resolveDelegate = Services.ResolveDelegateFactory.Create(typeKey, this);
                resolveDelegate = _resolveDelegates.GetOrAdd(typeKey, resolveDelegate);
            }
            return resolveDelegate(Services.ResolveContextFactory.Create(this));
        }

        public Registration GetRegistration(TypeKey typeKey)
        {
            // TODO: check if generic type is asked
            if (_registrationMappings.TryGetValue(typeKey.Type, out var registrations) && registrations.TryFindRegistration(typeKey, out var targetRegistration))
                return targetRegistration;

            targetRegistration = Services.AutomaticRegistrationFactory.CreateDefaultRegistration(typeKey, this);
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