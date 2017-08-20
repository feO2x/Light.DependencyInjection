using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ResolveOnlyRegistrationsAlgorithm : BaseRegistrationOrResolveAllAlgorithm
    {
        protected override ResolveInfo FindForCollectionType(TypeKey requestedTypeKey, DiContainer container, Type closedConstructedIEnumerableType, Registration existingGenericCollectionRegistration)
        {
            if (existingGenericCollectionRegistration == null)
                throw new ResolveException($"There is no registration present for type {requestedTypeKey}.");

            return new ResolveRegistrationInfo(requestedTypeKey, existingGenericCollectionRegistration);
        }
    }
}