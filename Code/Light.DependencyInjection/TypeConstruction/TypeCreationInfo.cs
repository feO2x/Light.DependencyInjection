using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeCreationInfo
    {
        private readonly InstanceInjection[] _instanceInjections;
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeCreationInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, InstanceInjection[] instanceInjections = null)
        {
            instantiationInfo.MustNotBeNull(nameof(instantiationInfo));
            CheckInstantiationInfoType(typeKey, instantiationInfo);
            CheckInstanceInjectionTypes(typeKey, instanceInjections);

            TypeKey = typeKey;
            InstantiationInfo = instantiationInfo;
            _instanceInjections = instanceInjections;
        }

        public Type TargetType => TypeKey.Type;
        public TypeInfo TargetTypeInfo => InstantiationInfo.TargetTypeInfo;

        public IReadOnlyList<InstanceInjection> InstanceInjections => _instanceInjections;

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckInstantiationInfoType(TypeKey typeKey, InstantiationInfo instantiationInfo)
        {
            if (instantiationInfo.TargetType == typeKey.Type)
                return;

            throw new ArgumentException($"The instantiation info that was injected into the type creation info for type {typeKey.GetFullRegistrationName()} was not created for the same type (\"{instantiationInfo.TargetType}\").", nameof(instantiationInfo));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckInstanceInjectionTypes(TypeKey typeKey, InstanceInjection[] instanceInjections)
        {
            if (instanceInjections == null || instanceInjections.Length == 0)
                return;

            foreach (var injection in instanceInjections)
            {
                if (injection.DeclaringType == typeKey.Type)
                    continue;

                throw new ArgumentException($"The declaring type of \"{injection}\" does not fit the type creation info {typeKey.GetFullRegistrationName()}.", nameof(instanceInjections));
            }
        }

        public object CreateInstance(CreationContext context)
        {
            var instance = InstantiationInfo.Instantiate(context);
            PerformInstanceInjections(instance, context);
            if (context.Registration.IsTrackingDisposables)
                context.Container.Scope.TryAddDisposable(instance);
            return instance;
        }

        private void PerformInstanceInjections(object instance, CreationContext context)
        {
            // Perform injections for members that were configured
            if (_instanceInjections != null && _instanceInjections.Length > 0)
            {
                foreach (var instanceInjection in _instanceInjections)
                    instanceInjection.InjectValue(instance, context);
            }

            // Check if there are injections on members that are not configured with the Di Container
            if (context.ParameterOverrides == null || context.ParameterOverrides.Value.AdditionalInjections == null)
                return;


            var additionalInjections = context.ParameterOverrides.Value.AdditionalInjections;
            for (var i = 0; i < additionalInjections.Count; i++)
            {
                var injectionDescription = additionalInjections[i];
                context.Container.Services.InjectorForUnknownInstanceMembers.InjectValue(injectionDescription.MemberInfo, instance, injectionDescription.Value);
            }
        }

        public TypeCreationInfo BindToClosedGenericType(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            if (_instanceInjections == null || _instanceInjections.Length == 0)
                return new TypeCreationInfo(new TypeKey(closedGenericType, TypeKey.RegistrationName), InstantiationInfo.BindToClosedGenericType(closedGenericType, closedGenericTypeInfo));

            var instanceInjections = new InstanceInjection[_instanceInjections.Length];
            for (var i = 0; i < _instanceInjections.Length; i++)
            {
                instanceInjections[i] = _instanceInjections[i].BindToClosedGenericType(closedGenericType, closedGenericTypeInfo);
            }
            return new TypeCreationInfo(new TypeKey(closedGenericType, TypeKey.RegistrationName), InstantiationInfo.BindToClosedGenericType(closedGenericType, closedGenericTypeInfo), instanceInjections);
        }
    }
}