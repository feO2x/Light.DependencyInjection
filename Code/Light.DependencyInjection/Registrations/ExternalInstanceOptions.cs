using System;
using System.Collections.Generic;
using Light.DependencyInjection.Lifetimes;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ExternalInstanceOptions : CommonRegistrationOptions<IExternalInstanceOptions>, IExternalInstanceOptions
    {
        private readonly Lifetime _lifeTime;

        public ExternalInstanceOptions(object value, Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes) : base(value.MustNotBeNull(nameof(value)).GetType(), ignoredAbstractionTypes)
        {
            _lifeTime = new ExternalInstanceLifetime(value);
        }

        public ExternalInstanceOptions(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes) : base(targetType, ignoredAbstractionTypes)
        {
            _lifeTime = ScopedExternalInstanceLifetime.Instance;
        }

        public Registration CreateRegistration()
        {
            return new Registration(new TypeKey(TargetType, RegistrationName),
                                    _lifeTime,
                                    mappedAbstractionTypes: CreateMappedAbstractionsList(),
                                    isTrackingDisposables: IsTrackingDisposables);
        }
    }
}