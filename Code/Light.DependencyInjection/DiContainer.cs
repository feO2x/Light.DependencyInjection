using System;
using System.Collections.Generic;
using Light.DependencyInjection.DataStructures;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public class DiContainer
    {
        private readonly IConcurrentDictionary<Type, IConcurrentList<Registration>> _registrationMapping;
        private readonly IConcurrentDictionary<TypeKey, Func<object>> _standardizedConstructionFunctions;
        private ContainerServices _services;

        public DiContainer(ContainerServices services = null)
        {
            _services = services ?? new ContainerServices();
            _registrationMapping = _services.ConcurrentDictionaryFactory.Create<Type, IConcurrentList<Registration>>();
            _standardizedConstructionFunctions = _services.ConcurrentDictionaryFactory.Create<TypeKey, Func<object>>();
        }

        public ContainerServices Services
        {
            get => _services;
            set => _services = value.MustNotBeNull();
        }

        public DiContainer Register(Registration registration)
        {
            registration.MustNotBeNull();

            if (_registrationMapping.TryGetValue(registration.TargetType, out var typeRegistrations) == false)
            {
                typeRegistrations = _services.ConcurrentListFactory.Create<Registration>();
                typeRegistrations = _registrationMapping.GetOrAdd(registration.TypeKey, typeRegistrations);
            }

            typeRegistrations.AddOrUpdate(registration);
            return this;
        }

        public DiContainer Register(Registration registration, IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            registration.MustNotBeNull(nameof(registration));
            abstractionTypes.MustNotBeNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                // TODO: Check if the abstraction type actually maps to the concrete type
                //registration.TargetType.MustInheritFromOrImplement(abstractionType);
                if (_registrationMapping.TryGetValue(abstractionType, out var typeRegistrations) == false)
                {
                    typeRegistrations = _services.ConcurrentListFactory.Create<Registration>();
                    typeRegistrations = _registrationMapping.GetOrAdd(abstractionType, typeRegistrations);
                }
                typeRegistrations.AddOrUpdate(registration);
            }
            // ReSharper restore PossibleMultipleEnumeration

            Register(registration);
            return this;
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

            if (_standardizedConstructionFunctions.TryGetValue(typeKey, out var standardizedConstructionFunction))
                return standardizedConstructionFunction();

            standardizedConstructionFunction = _services.StandardizedConstructionFunctionFactory.Create(typeKey, this);
            standardizedConstructionFunction = _standardizedConstructionFunctions.GetOrAdd(typeKey, standardizedConstructionFunction);
            return standardizedConstructionFunction();
        }

        public Registration GetRegistration(TypeKey typeKey)
        {
            // TODO: check if generic type is asked
            if (_registrationMapping.TryGetValue(typeKey.Type, out var registrations) == false ||
                registrations.TryFindRegistration(typeKey, out var targetRegistration) == false)
                throw new ResolveException($"There is no registration for {typeKey}");

            return targetRegistration;
        }
    }
}