using System;
using System.Collections.Generic;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ResolveAllInfo : ResolveInfo
    {
        public readonly IReadOnlyList<Registration> Registrations;
        public readonly Type ItemType;

        public ResolveAllInfo(TypeKey requestedTypeKey, IReadOnlyList<Registration> registrations, Type itemType) : base(requestedTypeKey)
        {
            registrations.MustNotBeNullOrEmpty(nameof(registrations));

            Registrations = registrations;
            ItemType = itemType.MustNotBeNull(nameof(itemType));
        }

        public Type CollectionType => RequestedTypeKey.Type;
    }
}