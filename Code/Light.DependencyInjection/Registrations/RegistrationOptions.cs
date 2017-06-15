using System;
using System.Collections.Generic;
using System.Reflection;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public class RegistrationOptions<TConcrete> : IRegistrationOptions<TConcrete>
    {
        private readonly IDefaultInstantiationInfoSelector _defaultInstantiationInfoSelector;
        private readonly string _registrationName = "";
        private readonly HashSet<Type> _abstractionTypes = new HashSet<Type>();

        public RegistrationOptions(IDefaultInstantiationInfoSelector defaultInstantiationInfoSelector)
        {
            _defaultInstantiationInfoSelector = defaultInstantiationInfoSelector.MustNotBeNull(nameof(defaultInstantiationInfoSelector));
        }

        public DiContainer PerformRegistration(DiContainer container, Lifetime lifetime, Action<IRegistrationOptions<TConcrete>> configureRegistration = null)
        {
            container.MustNotBeNull();

            configureRegistration?.Invoke(this);

            container.Register(CreateRegistration(lifetime), _abstractionTypes);
            return container;
        }

        public Registration CreateRegistration(Lifetime lifeTime)
        {
            lifeTime.MustNotBeNull(nameof(lifeTime));

            var typeKey = new TypeKey(typeof(TConcrete), _registrationName);
            var typeConstructionInfo = default(TypeConstructionInfo);
            if (lifeTime.IsCreatingNewInstances)
            {
                var instantiationInfoFactory = _defaultInstantiationInfoSelector.GetDefaultInstantiationInfo(typeKey.Type.GetTypeInfo());
                typeConstructionInfo = new TypeConstructionInfo(typeKey, instantiationInfoFactory.Create(_registrationName));
            }

            return new Registration(typeKey, lifeTime, typeConstructionInfo);
        }

        public RegistrationOptions<TConcrete> MapToAbstractions(params Type[] abstractionTypes)
        {
            foreach (var abstractionType in abstractionTypes)
            {
                _abstractionTypes.Add(abstractionType);
            }
            
            return this;
        }
        
    }
}