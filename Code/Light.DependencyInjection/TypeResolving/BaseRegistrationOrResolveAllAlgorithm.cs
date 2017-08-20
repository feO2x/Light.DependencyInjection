using System;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public abstract class BaseRegistrationOrResolveAllAlgorithm : IResolveInfoAlgorithm
    {
        public ResolveInfo Search(TypeKey requestedTypeKey, DiContainer container, bool? tryResolveAll)
        {
            requestedTypeKey.MustNotBeEmpty(nameof(requestedTypeKey));
            container.MustNotBeNull(nameof(container));

            if (tryResolveAll == false || requestedTypeKey.RegistrationName != string.Empty)
                return FindRegistration(requestedTypeKey, container);

            var closedConstructedIEnumerableType = requestedTypeKey.Type.FindClosedConstructedIEnumerableType();
            if (tryResolveAll == true && closedConstructedIEnumerableType != null)
                return FindAllRegistrations(requestedTypeKey.Type, GetItemTypeOfCollection(closedConstructedIEnumerableType, requestedTypeKey), container);
            if (closedConstructedIEnumerableType == null)
                return FindRegistration(requestedTypeKey, container);

            var existingRegistration = container.GetRegistration(requestedTypeKey);
            if (existingRegistration != null && existingRegistration.IsGenericRegistration == false)
                return new ResolveRegistrationInfo(requestedTypeKey, existingRegistration);

            return FindForCollectionType(requestedTypeKey, container, closedConstructedIEnumerableType, existingRegistration);
        }

        protected abstract ResolveInfo FindForCollectionType(TypeKey requestedTypeKey, DiContainer container, Type closedConstructedIEnumerableType, Registration existingGenericCollectionRegistration);

        private static ResolveRegistrationInfo FindRegistration(TypeKey requestedTypeKey, DiContainer container)
        {
            var registration = container.GetRegistration(requestedTypeKey);
            if (registration != null)
                return new ResolveRegistrationInfo(requestedTypeKey, registration);

            throw new ResolveException($"There is no registration present for type {requestedTypeKey}.");
        }

        private static ResolveAllInfo FindAllRegistrations(Type requestedCollectionType, Type itemType, DiContainer container)
        {
            var registrations = container.GetRegistrationsForType(requestedCollectionType);
            if (registrations != null)
                return new ResolveAllInfo(new TypeKey(requestedCollectionType), registrations, itemType);

            throw new ResolveException($"There are no registrations present for type \"{requestedCollectionType}\".");
        }

        protected static Type GetItemTypeOfCollection(Type closedConstructedIEnumerableType, TypeKey requestedTypeKey)
        {
            if (closedConstructedIEnumerableType == null)
                throw new ResolveException($"Cannot perform a ResolveAll on type {requestedTypeKey} because this is not a collection type.");

            return closedConstructedIEnumerableType.GenericTypeArguments[0];
        }
    }
}