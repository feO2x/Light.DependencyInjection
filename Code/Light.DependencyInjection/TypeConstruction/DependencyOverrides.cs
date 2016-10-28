using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents a structure that allows you to override the dependencies of the top level resolved object.
    /// </summary>
    public struct DependencyOverrides
    {
        /// <summary>
        ///     Gets the type creation info for the target type.
        /// </summary>
        public readonly TypeCreationInfo TypeCreationInfo;

        /// <summary>
        ///     Gets the array that will be passed when calling the Standardized Instantiation Function.
        /// </summary>
        public readonly object[] InstantiationParameters;

        private Dictionary<InstanceInjection, object> _instanceInjectionOverrides;
        private List<UnknownInjectionDescription> _additionalInjections;

        /// <summary>
        ///     Gets the dictionary containing all overrides for instance injections.
        /// </summary>
        public IReadOnlyDictionary<InstanceInjection, object> InstanceInjectionOverrides => _instanceInjectionOverrides;

        /// <summary>
        ///     Gets the list of additional injections (i.e. injections that could not be mapped to existing <see cref="InstanceInjection" /> objects).
        /// </summary>
        public IReadOnlyList<UnknownInjectionDescription> AdditionalInjections => _additionalInjections;

        /// <summary>
        ///     Initializes a new instance of <see cref="DependencyOverrides" />.
        /// </summary>
        /// <param name="typeCreationInfo">The type creation info of the target type whose dependencies should be overridden.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeCreationInfo" /> is null.</exception>
        public DependencyOverrides(TypeCreationInfo typeCreationInfo)
        {
            typeCreationInfo.MustNotBeNull(nameof(typeCreationInfo));

            TypeCreationInfo = typeCreationInfo;

            var instantiationDependencies = typeCreationInfo.InstantiationInfo.InstantiationDependencies;
            InstantiationParameters = instantiationDependencies == null || instantiationDependencies.Count == 0 ? null : new object[instantiationDependencies.Count];
            _instanceInjectionOverrides = null;
            _additionalInjections = null;
        }

        /// <summary>
        ///     Injects the given value into the parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to be injected into the Standardized Instantiation Function.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterName" /> is null.</exception>
        /// #
        /// <exception cref="EmptyStringException">Thrown when <paramref name="parameterName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="parameterName" />contains only whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName" /> is not a valid parameter name of the instantiation function.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there are no Standardized Instantiation Function takes no parameters.</exception>
        public DependencyOverrides OverrideInstantiationParameter(string parameterName, object value)
        {
            parameterName.MustNotBeNullOrWhiteSpace(nameof(parameterName));
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

        /// <summary>
        ///     Injects the given value into the parameter with the specified unique type.
        /// </summary>
        /// <param name="type">The type of the parameter. This type must be unique within the parameter list.</param>
        /// <param name="value">The value to be injected into the Standardized Instantiation Function.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="type" /> is not a unique type in the parameter list or when there is no parameter with the given type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there are no Standardized Instantiation Function takes no parameters.</exception>
        public DependencyOverrides OverrideInstantiationParameter(Type type, object value)
        {
            type.MustNotBeNull(nameof(type));
            EnsureInstantiationParameterNotNull();

            var targetIndex = FindSingleTargetIndexByType(type);
            InstantiationParameters[targetIndex] = value.EscapeNullIfNecessary();
            return this;
        }

        /// <summary>
        ///     Injects the given value into the parameter with the specified unique type.
        /// </summary>
        /// <typeparam name="T">The type of the parameter. This type must be unique within the parameter list.</typeparam>
        /// <param name="value">The value to be injected into the Standardized Instantiation Function.</param>
        /// <exception cref="ArgumentException">Thrown when the specified type is not a unique type in the parameter list or when there is no parameter with the given type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there are no Standardized Instantiation Function takes no parameters.</exception>
        public DependencyOverrides OverrideInstantiationParameter<T>(object value)
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

        /// <summary>
        ///     Injects the given value into the specified member after the target instance was created.
        /// </summary>
        /// <param name="memberName">The name of the target member.</param>
        /// <param name="value">The value to be injected into the member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName" /> is null.</exception>
        /// #
        /// <exception cref="EmptyStringException">Thrown when <paramref name="memberName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="memberName" />contains only whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when the target type does not contain a member with the given name.</exception>
        public DependencyOverrides OverrideMember(string memberName, object value)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));

            var instanceInjection = FindInstanceInjection(memberName);
            if (TryAddInstanceInjectionOverride(instanceInjection, value))
                return this;

            var targetMember = TypeCreationInfo.TargetTypeInfo.DeclaredMembers.FirstOrDefault(m => m.Name == memberName);
            if (targetMember == null)
                throw new ArgumentException($"The type \"{TypeCreationInfo.TargetType}\" does not contain a member called \"{memberName}\".", nameof(memberName));

            AddAdditionalInjections(targetMember, value);
            return this;
        }

        /// <summary>
        ///     Injects the given value into the specified member after the target instance was created.
        /// </summary>
        /// <param name="memberInfo">The info describing the target member.</param>
        /// <param name="value">The value to be injected into the member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo" /> is null.</exception>
        /// <exception cref="ArgumentNullException">Throw when the declaring type of <paramref name="memberInfo" /> is not the target type.</exception>
        public DependencyOverrides OverrideMember(MemberInfo memberInfo, object value)
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
}