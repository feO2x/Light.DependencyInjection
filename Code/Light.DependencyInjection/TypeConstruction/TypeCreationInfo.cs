using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents the metadata neceassary to instantiate and populate the type uniquely identified by the type key.
    /// </summary>
    public sealed class TypeCreationInfo
    {
        private readonly InstanceInjection[] _instanceInjections;

        /// <summary>
        ///     Gets the info describing how the target type is instantiated with the Standardized Instantiation Function.
        /// </summary>
        public readonly InstantiationInfo InstantiationInfo;

        /// <summary>
        ///     Gets the type key uniquely identifying this type creation info.
        /// </summary>
        public readonly TypeKey TypeKey;

        /// <summary>
        ///     Initializes a new instance of <see cref="TypeCreationInfo" />.
        /// </summary>
        /// <param name="typeKey">The type key uniquely identifying this info.</param>
        /// <param name="instantiationInfo">The info describing how the target type is instantiated.</param>
        /// <param name="instanceInjections">The instance injections that will be applied after the target type was instantiated. Can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instantiationInfo" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the target type of the <paramref name="instantiationInfo" /> or the declaring type of the <paramref name="instanceInjections" /> is not the same as the one in <paramref name="typeKey" />.</exception>
        public TypeCreationInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, InstanceInjection[] instanceInjections = null)
        {
            instantiationInfo.MustNotBeNull(nameof(instantiationInfo));
            CheckInstantiationInfoType(typeKey, instantiationInfo);
            CheckInstanceInjectionTypes(typeKey, instanceInjections);

            TypeKey = typeKey;
            InstantiationInfo = instantiationInfo;
            _instanceInjections = instanceInjections;
        }

        /// <summary>
        ///     Gets the target type of this creation info.
        /// </summary>
        public Type TargetType => TypeKey.Type;

        /// <summary>
        ///     Gets the type info of the target type.
        /// </summary>
        public TypeInfo TargetTypeInfo => InstantiationInfo.TargetTypeInfo;

        /// <summary>
        ///     Gets the list of instance injections that will be applied after the target type was created. Can be null.
        /// </summary>
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

        /// <summary>
        ///     Creates an instance of the target type using the <see cref="InstantiationInfo" />,
        ///     applies <see cref="InstanceInjections" /> if present, and adds the created instance
        ///     to the DI container scope for disposable tracking if this was configured and
        ///     the target type implements <see cref="IDisposable" />.
        /// </summary>
        /// <param name="context">The context used to resolve child values.</param>
        /// <returns>A newly created instance of the target type.</returns>
        public object CreateInstance(ResolveContext context)
        {
            var instance = InstantiationInfo.Instantiate(context);
            PerformInstanceInjections(instance, context);
            if (context.Registration.IsTrackingDisposables)
                context.Container.Scope.TryAddDisposable(instance);
            return instance;
        }

        private void PerformInstanceInjections(object instance, ResolveContext context)
        {
            // Perform injections for members that were configured
            if (_instanceInjections != null && _instanceInjections.Length > 0)
            {
                foreach (var instanceInjection in _instanceInjections)
                    instanceInjection.InjectValue(instance, context);
            }

            // Check if there are injections on members that are not configured with the Di Container
            if (context.DependencyOverrides == null || context.DependencyOverrides.Value.AdditionalInjections == null)
                return;


            var additionalInjections = context.DependencyOverrides.Value.AdditionalInjections;
            for (var i = 0; i < additionalInjections.Count; i++)
            {
                var injectionDescription = additionalInjections[i];
                context.Container.Services.InjectorForUnknownInstanceMembers.InjectValue(injectionDescription.MemberInfo, instance, injectionDescription.Value);
            }
        }

        /// <summary>
        ///     Creates a new <see cref="TypeCreationInfo" /> for the <paramref name="closedGenericType" /> with the
        ///     settings copied from this instance. You may only call this method if the target type is a generic type definition.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="closedGenericType" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the target type is not a generic type definition.</exception>
        /// <exception cref="ResolveTypeException">Thrown when <paramref name="closedGenericType" />is not a closed generic type variant of the target type.</exception>
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