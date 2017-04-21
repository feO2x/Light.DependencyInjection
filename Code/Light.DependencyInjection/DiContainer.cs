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
        private readonly IConcurrentDictionary<Type, IList<Registration>> _registrations;
        private ContainerServices _services;

        public DiContainer(ContainerServices services)
        {
            services.MustNotBeNull(nameof(services));

            _services = services;
            _registrations = services.ConcurrentDictionaryFactory.Create<Type, IList<Registration>>();
        }

        public ContainerServices Services
        {
            get => _services;
            set
            {
                value.MustNotBeNull();
                _services = value;
            }
        }
    }
}