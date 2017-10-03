using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ResolveOnlyRegistrationsAlgorithm : RegistrationOrResolveAllAlgorithm
    {
        protected override ResolveInfo FindForCollectionType(TypeKey requestedTypeKey,
                                                             DependencyInjectionContainer container,
                                                             Type closedConstructedIEnumerableType,
                                                             Registration existingGenericCollectionRegistration)
        {
            if (existingGenericCollectionRegistration == null)
                throw new ResolveException($"There is no registration present for type {requestedTypeKey}.");

            return new ResolveRegistrationInfo(requestedTypeKey, existingGenericCollectionRegistration);
        }
    }
}