using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    /// <summary>
    ///     Represents the abstraction for configuring registrations that return an externally created instance (i.e. the DI container does not create instances of the corresponding type).
    /// </summary>
    /// <typeparam name="TConcreteOptions">The options type that is returned by the fluent API.</typeparam>
    public abstract class BaseRegistrationOptionsForExternalInstance<TConcreteOptions> : IBaseRegistrationOptionsForExternalInstance<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
    {
        /// <summary>
        ///     Gets the abstraction types that are mapped to the target type.
        /// </summary>
        protected readonly HashSet<Type> AbstractionTypes = new HashSet<Type>();

        /// <summary>
        ///     Gets the list of abstraction types that are ignored when abstraction types are mapped to the target type.
        /// </summary>
        protected readonly IReadOnlyList<Type> IgnoredAbstractionTypes;

        /// <summary>
        ///     Gets the target type of these options.
        /// </summary>
        protected readonly Type TargetType;

        /// <summary>
        ///     Gets the type info of the target type.
        /// </summary>
        protected readonly TypeInfo TargetTypeInfo;

        /// <summary>
        ///     Gets the this reference cast to TConcreteOptions.
        /// </summary>
        protected readonly TConcreteOptions This;

        /// <summary>
        ///     Initializes a new instance of <see cref="BaseRegistrationOptionsForExternalInstance{TConcreteOptions}" />.
        /// </summary>
        /// <param name="targetType">The target type of these options.</param>
        /// <param name="ignoredAbstractionTypes">The list of abstraction types that are ignored when abstraction types are mapped to the target type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> or <paramref name="ignoredAbstractionTypes" /> is null.</exception>
        protected BaseRegistrationOptionsForExternalInstance(Type targetType,
                                                             IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));

            TargetType = targetType;
            TargetTypeInfo = targetType.GetTypeInfo();
            IgnoredAbstractionTypes = ignoredAbstractionTypes;
            This = this as TConcreteOptions;
            EnsureThisIsNotNull();
        }

        /// <summary>
        ///     Gets the abstraction types that map to the target type.
        /// </summary>
        public IEnumerable<Type> MappedAbstractionTypes => AbstractionTypes;

        /// <summary>
        ///     Gets the value indicating whether the DI container should track <see cref="IDisposable" /> instances of the target type.
        /// </summary>
        public bool IsContainerTrackingDisposables { get; protected set; } = true;

        /// <summary>
        ///     Gets the name of the target registration.
        /// </summary>
        public string RegistrationName { get; protected set; }

        /// <inheritdoc />
        public TConcreteOptions UseRegistrationName(string registrationName)
        {
            RegistrationName = registrationName;
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions DisableIDisposableTrackingForThisType()
        {
            IsContainerTrackingDisposables = false;
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions MapToAllImplementedInterfaces()
        {
            return MapToAbstractions(TargetTypeInfo.ImplementedInterfaces);
        }

        /// <inheritdoc />
        public TConcreteOptions MapToAbstractions(IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            abstractionTypes.MustNotBeNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                // Check if the type is a open constructed generic type, if yes then get its generic type definition
                var targetAbstractionType = AdjustGenericAbstractionTypeIfNecessary(abstractionType);
                if (IgnoredAbstractionTypes.Contains(targetAbstractionType))
                    continue;

                AbstractionTypes.Add(targetAbstractionType);
            }
            return This;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <inheritdoc />
        public TConcreteOptions MapToAbstractions(params Type[] abstractionTypes)
        {
            return MapToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        /// <inheritdoc />
        public TConcreteOptions UseTypeNameAsRegistrationName()
        {
            RegistrationName = TargetType.Name;
            return This;
        }

        /// <inheritdoc />
        public TConcreteOptions UseFullTypeNameAsRegistrationName()
        {
            RegistrationName = TargetType.FullName;
            return This;
        }

        private Type AdjustGenericAbstractionTypeIfNecessary(Type abstractionType)
        {
            var typeInfo = abstractionType.GetTypeInfo();
            if (typeInfo.IsGenericParameter)
                throw new TypeRegistrationException($"The type {abstractionType} is a generic parameter and cannot be used as an abstraction for {TargetType}.", TargetType);

            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters && typeInfo.IsGenericTypeDefinition == false)
                return typeInfo.GetGenericTypeDefinition();

            return abstractionType;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureThisIsNotNull()
        {
            if (This != null)
                return;

            throw new InvalidOperationException($"The class {GetType()} does not implement {typeof(TConcreteOptions)}.");
        }
    }
}