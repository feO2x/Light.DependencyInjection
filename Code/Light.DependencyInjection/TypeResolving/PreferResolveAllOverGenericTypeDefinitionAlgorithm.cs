using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class PreferResolveAllOverGenericTypeDefinitionAlgorithm : RegistrationOrResolveAllAlgorithm
    {
        protected override ResolveInfo FindForCollectionType(TypeKey requestedTypeKey, DependencyInjectionContainer container, Type closedConstructedIEnumerableType, Registration existingGenericCollectionRegistration)
        {
            var itemType = GetItemTypeOfCollection(closedConstructedIEnumerableType, requestedTypeKey);
            var registrations = container.GetRegistrationsForType(itemType);
            if (registrations != null)
                return new ResolveAllInfo(requestedTypeKey, registrations, itemType);

            if (existingGenericCollectionRegistration == null)
                throw new ResolveException($"There is no registration present for type {requestedTypeKey}.");

            return new ResolveRegistrationInfo(requestedTypeKey, existingGenericCollectionRegistration);
        }
    }
}