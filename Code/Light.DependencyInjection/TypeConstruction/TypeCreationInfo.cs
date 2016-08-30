using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeCreationInfo
    {
        public readonly Type TargetType;
        public readonly InstantiationInfo InstantiationInfo;
        private readonly List<InstanceInjection> _instanceInjections;

        public TypeCreationInfo(InstantiationInfo instantiationInfo, List<InstanceInjection> instanceInjections = null)
        {
            instantiationInfo.MustNotBeNull(nameof(instantiationInfo));

            InstantiationInfo = instantiationInfo;
            _instanceInjections = instanceInjections;
            TargetType = instantiationInfo.TargetType;
        }

        public IReadOnlyList<InstanceInjection> InstanceInjections => _instanceInjections;

        public object CreateInstance(DiContainer container)
        {
            var instance = InstantiationInfo.Instantiate(container);
            if (_instanceInjections == null || _instanceInjections.Count == 0)
                return instance;

            foreach (var instanceInjection in _instanceInjections)
            {
                var injectionValue = container.Resolve(instanceInjection.MemberType, instanceInjection.ChildValueRegistrationName);
                instanceInjection.InjectValue(instance, injectionValue);
            }
            return instance;
        }

        public TypeCreationInfo CloneForBoundGenericType(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            if (_instanceInjections == null || _instanceInjections.Count <= 0)
                return new TypeCreationInfo(InstantiationInfo.CloneForBoundGenericType(boundGenericType, boundGenericTypeInfo));

            var instanceInjections = new List<InstanceInjection>();
            foreach (var instanceInjection in _instanceInjections)
            {
                instanceInjections.Add(instanceInjection.CloneForBoundGenericType(boundGenericType, boundGenericTypeInfo));
            }
            return new TypeCreationInfo(InstantiationInfo.CloneForBoundGenericType(boundGenericType, boundGenericTypeInfo), instanceInjections);
        }
    }
}