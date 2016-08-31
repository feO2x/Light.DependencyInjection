﻿using System;
using System.Reflection;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TransientRegistration : Registration
    {
        public TransientRegistration(TypeCreationInfo typeCreationInfo, string registrationName = null)
            : base(new TypeKey(typeCreationInfo.TargetType, registrationName), typeCreationInfo) { }

        protected override object GetInstanceInternal(DiContainer container)
        {
            return TypeCreationInfo.CreateInstance(container);
        }

        protected override Registration BindGenericTypeDefinition(Type closedConstructedGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new TransientRegistration(TypeCreationInfo.CloneForClosedConstructedGenericType(closedConstructedGenericType, boundGenericTypeInfo),
                                             TypeKey.RegistrationName);
        }
    }
}