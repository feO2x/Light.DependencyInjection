using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ExternalInstanceOptions : BaseExternalInstanceOptions<IExternalInstanceOptions>, IExternalInstanceOptions
    {
        private readonly ExternalInstanceLifetime _lifeTime;

        public ExternalInstanceOptions(object value, IReadOnlyList<Type> ignoredAbstractionTypes) : base(value.MustNotBeNull(nameof(value)).GetType(), ignoredAbstractionTypes)
        {
            _lifeTime = new ExternalInstanceLifetime(value);
        }

        public Registration CreateRegistration()
        {
            return new Registration(new TypeKey(TargetType, RegistrationName), _lifeTime, isTrackingDisposables: IsTrackingDisposables);
        }
    }
}