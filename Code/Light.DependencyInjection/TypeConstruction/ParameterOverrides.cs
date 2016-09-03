using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct ParameterOverrides
    {
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly object[] InstantiationParameters;
        private Dictionary<InstanceInjection, object> _instanceInjectionOverrides;
        private List<UnknownInjectionDescription> _additionalInjections;

        public IReadOnlyDictionary<InstanceInjection, object> InstanceInjectionOverrides => _instanceInjectionOverrides;

        public IReadOnlyList<UnknownInjectionDescription> AdditionalInjections => _additionalInjections;

        public ParameterOverrides(TypeCreationInfo typeCreationInfo)
        {
            typeCreationInfo.MustNotBeNull(nameof(typeCreationInfo));

            TypeCreationInfo = typeCreationInfo;

            var instantiationDependencies = typeCreationInfo.InstantiationInfo.InstantiationDependencies;
            InstantiationParameters = instantiationDependencies == null || instantiationDependencies.Count == 0 ? null : new object[instantiationDependencies.Count];
            _instanceInjectionOverrides = null;
            _additionalInjections = null;
        }

        public ParameterOverrides OverrideInstantiationParameter(string parameterName, object value)
        {
            EnsureInstantiationParameterNotNull();

            for (var i = 0; i < TypeCreationInfo.InstantiationInfo.InstantiationDependencies.Count; i++)
            {
                if (TypeCreationInfo.InstantiationInfo.InstantiationDependencies[i].TargetParameter.Name != parameterName)
                    continue;

                InstantiationParameters[i] = value.EscapeNullIfNecessary();
                return this;
            }

            throw new ArgumentException($"There is no parameter with name \"{parameterName}\" for the instantiation method of type {TypeCreationInfo.TypeKey.GetFullRegistrationName()}.", nameof(parameterName));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureInstantiationParameterNotNull()
        {
            if (InstantiationParameters == null)
                throw new InvalidOperationException($"You cannot override an instantiation method parameter for type \"{TypeCreationInfo.TargetType}\" because the instantiation method is parametersless.");
        }

        public ParameterOverrides OverrideInstantiationParameter(Type type, object value)
        {
            type.MustNotBeNull(nameof(type));
            EnsureInstantiationParameterNotNull();

            var targetIndex = FindSingleTargetIndexByType(type);
            InstantiationParameters[targetIndex] = value.EscapeNullIfNecessary();
            return this;
        }

        public ParameterOverrides OverrideInstantiationParameter<T>(object value)
        {
            OverrideInstantiationParameter(typeof(T), value);
            return this;
        }

        private int FindSingleTargetIndexByType(Type parameterType)
        {
            var targetIndex = -1;
            for (var i = 0; i < TypeCreationInfo.InstantiationInfo.InstantiationDependencies.Count; ++i)
            {
                if (TypeCreationInfo.InstantiationInfo.InstantiationDependencies[i].ParameterType != parameterType)
                    continue;

                if (targetIndex != -1)
                    throw new ArgumentException($"You cannot override the parameter with type \"{parameterType}\" because there are several parameters with this type in the instantiation method for type {TypeCreationInfo.TypeKey.GetFullRegistrationName()}.", nameof(parameterType));

                targetIndex = i;
            }

            if (targetIndex == -1)
                throw new ArgumentException($"The instantiation method of type {TypeCreationInfo.TypeKey.GetFullRegistrationName()} has no parameter with type \"{parameterType}\".", nameof(parameterType));

            return targetIndex;
        }

        public ParameterOverrides OverrideMember(string memberName, object value)
        {
            var instanceInjection = FindInstanceInjection(memberName);
            if (TryAddInstanceInjectionOverride(instanceInjection, value))
                return this;

            var targetMember = TypeCreationInfo.TargetTypeInfo.DeclaredMembers.FirstOrDefault(m => m.Name == memberName);
            if (targetMember == null)
                throw new ArgumentException($"The type \"{TypeCreationInfo.TargetType}\" does not contain a member called \"{memberName}\".", nameof(memberName));

            AddAdditionalInjections(targetMember, value);
            return this;
        }

        public ParameterOverrides OverrideMember(MemberInfo memberInfo, object value)
        {
            CheckMemberInfo(memberInfo);

            var instanceInjection = FindInstanceInjection(memberInfo.Name);
            if (TryAddInstanceInjectionOverride(instanceInjection, value))
                return this;

            AddAdditionalInjections(memberInfo, value);
            return this;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckMemberInfo(MemberInfo memberInfo)
        {
            memberInfo.MustNotBeNull(nameof(memberInfo));
            if (memberInfo.DeclaringType != TypeCreationInfo.TargetType)
                throw new ArgumentException($"The specified MemberInfo {memberInfo} does not describe a member of type {TypeCreationInfo.TargetType}.", nameof(memberInfo));
        }

        private InstanceInjection FindInstanceInjection(string instanceInjectionName)
        {
            if (TypeCreationInfo.InstanceInjections != null)
            {
                for (var i = 0; i < TypeCreationInfo.InstanceInjections.Count; ++i)
                {
                    var instanceInjection = TypeCreationInfo.InstanceInjections[i];
                    if (TypeCreationInfo.InstanceInjections[i].MemberName == instanceInjectionName)
                        return instanceInjection;
                }
            }

            return null;
        }

        private bool TryAddInstanceInjectionOverride(InstanceInjection instanceInjection, object value)
        {
            if (instanceInjection == null)
                return false;

            var instanceInjectionOverrides = GetInstanceInjectionOverrides();
            instanceInjectionOverrides.Add(instanceInjection, value.EscapeNullIfNecessary());
            return true;
        }

        private void AddAdditionalInjections(MemberInfo targetMember, object value)
        {
            var additionalInjections = GetAdditionalInjections();
            additionalInjections.Add(new UnknownInjectionDescription(targetMember, value.EscapeNullIfNecessary()));
        }

        private Dictionary<InstanceInjection, object> GetInstanceInjectionOverrides()
        {
            return _instanceInjectionOverrides ?? (_instanceInjectionOverrides = new Dictionary<InstanceInjection, object>());
        }

        private List<UnknownInjectionDescription> GetAdditionalInjections()
        {
            return _additionalInjections ?? (_additionalInjections = new List<UnknownInjectionDescription>());
        }
    }

    public static class ParameterOverridesExtensions
    {
        public static object EscapeNullIfNecessary(this object value)
        {
            return value ?? ExplicitlyPassedNull.Instance;
        }
    }
}