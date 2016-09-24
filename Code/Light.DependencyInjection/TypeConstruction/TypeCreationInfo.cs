using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeCreationInfo
    {
        private readonly List<InstanceInjection> _instanceInjections;
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeCreationInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, List<InstanceInjection> instanceInjections = null)
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
        private static void CheckInstanceInjectionTypes(TypeKey typeKey, List<InstanceInjection> instanceInjections)
        {
            if (instanceInjections == null || instanceInjections.Count == 0)
                return;

            foreach (var injection in instanceInjections)
            {
                if (injection.DeclaringType == typeKey.Type)
                    continue;

                throw new ArgumentException($"The declaring type of \"{injection}\" does not fit the type creation info {typeKey.GetFullRegistrationName()}.", nameof(instanceInjections));
            }
        }

        public object CreateInstance(DiContainer container, bool trackDisposable)
        {
            var instance = InstantiationInfo.Instantiate(container);
            if (_instanceInjections == null || _instanceInjections.Count == 0)
                return instance;

            foreach (var instanceInjection in _instanceInjections)
            {
                var injectionValue = container.Resolve(instanceInjection.MemberType, instanceInjection.ChildValueRegistrationName);
                instanceInjection.InjectValue(instance, injectionValue);
            }

            if (trackDisposable)
                container.Scope.TryAddDisposable(instance);
            return instance;
        }

        public object CreateInstance(DiContainer container, ParameterOverrides parameterOverrides, bool trackDisposable)
        {
            var instance = InstantiationInfo.Instantiate(container, parameterOverrides);
            if (_instanceInjections != null && _instanceInjections.Count > 0)
            {
                foreach (var instanceInjection in _instanceInjections)
                {
                    object injectionValue;
                    if (parameterOverrides.InstanceInjectionOverrides == null || parameterOverrides.InstanceInjectionOverrides.TryGetValue(instanceInjection, out injectionValue) == false)
                        injectionValue = container.Resolve(instanceInjection.MemberType, instanceInjection.ChildValueRegistrationName);
                    instanceInjection.InjectValue(instance, injectionValue);
                }
            }
            if (parameterOverrides.AdditionalInjections != null)
            {
                for (var i = 0; i < parameterOverrides.AdditionalInjections.Count; ++i)
                {
                    var simpleInjectionDescription = parameterOverrides.AdditionalInjections[i];
                    container.InjectorForUnknownInstanceMembers.InjectValue(simpleInjectionDescription.MemberInfo, instance, simpleInjectionDescription.Value);
                }
            }

            if (trackDisposable)
                container.Scope.TryAddDisposable(instance);

            return instance;
        }

        public TypeCreationInfo CloneForClosedConstructedGenericType(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            if (_instanceInjections == null || _instanceInjections.Count <= 0)
                return new TypeCreationInfo(new TypeKey(closedConstructedGenericType, TypeKey.RegistrationName), InstantiationInfo.CloneForClosedConstructedGenericType(closedConstructedGenericType, closedConstructedGenericTypeInfo));

            var instanceInjections = new List<InstanceInjection>();
            foreach (var instanceInjection in _instanceInjections)
            {
                instanceInjections.Add(instanceInjection.CloneForClosedConstructedGenericType(closedConstructedGenericType, closedConstructedGenericTypeInfo));
            }
            return new TypeCreationInfo(new TypeKey(closedConstructedGenericType, TypeKey.RegistrationName), InstantiationInfo.CloneForClosedConstructedGenericType(closedConstructedGenericType, closedConstructedGenericTypeInfo), instanceInjections);
        }
    }
}