using System;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public struct ResolveExpressionContext
    {
        public readonly TypeKey RequestedTypeKey;
        public readonly Registration Registration;
        public readonly DiContainer Container;
        public readonly Type ResolvedGenericRegistrationType;
        public readonly TypeInfo RegistrationTypeInfo;
        public readonly TypeInfo RequestedTypeInfo;
        public readonly TypeInfo ResolvedGenericRegistrationTypeInfo;

        public ResolveExpressionContext(TypeKey requestedTypeKey, Registration registration, DiContainer container)
        {
            RequestedTypeKey = requestedTypeKey.MustNotBeEmpty(nameof(requestedTypeKey));
            Registration = registration.MustNotBeNull(nameof(registration));
            Container = container.MustNotBeNull(nameof(container));
            RequestedTypeInfo = requestedTypeKey.Type.GetTypeInfo();
            RegistrationTypeInfo = registration.TypeKey.Type.GetTypeInfo();
            ResolvedGenericRegistrationType = null;
            ResolvedGenericRegistrationTypeInfo = null;

            // ReSharper disable once PossibleNullReferenceException
            if (RegistrationTypeInfo.IsGenericTypeDefinition == false)
                return;

            try
            {
                ResolvedGenericRegistrationType = registration.TargetType.MakeGenericType(RequestedType.GenericTypeArguments);
                ResolvedGenericRegistrationTypeInfo = ResolvedGenericRegistrationType.GetTypeInfo();
            }
            catch (Exception exception)
            {
                throw new ResolveException($"Could not resolve a concrete instance of the generic type definition \"{RegistrationType}\" using the type \"{RequestedType}\".", exception);
            }
        }

        public Type RequestedType => RequestedTypeKey.Type;
        public Type RegistrationType => Registration.TargetType;
        public Type InstanceType => ResolvedGenericRegistrationType ?? RegistrationType;
        public bool IsResolvingGenericTypeDefinition => RegistrationType.IsGenericTypeDefinition();

        public Type ResolveGenericTypeParameter(Type genericTypeParameter)
        {
            genericTypeParameter.MustNotBeNull(nameof(genericTypeParameter));

            for (var i = 0; i < RegistrationTypeInfo.GenericTypeParameters.Length; i++)
            {
                if (RegistrationTypeInfo.GenericTypeParameters[i] == genericTypeParameter)
                    return ResolvedGenericRegistrationTypeInfo.GenericTypeArguments[i];
            }

            throw new ArgumentException($"The type \"{genericTypeParameter}\" does not correspond to a generic parameter of type \"{RegistrationType}\".", nameof(genericTypeParameter));
        }
    }
}