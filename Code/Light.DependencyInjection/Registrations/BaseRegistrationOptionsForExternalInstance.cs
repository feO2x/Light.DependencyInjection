using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class BaseRegistrationOptionsForExternalInstance<TConcreteOptions> : IBaseRegistrationOptionsForExternalInstance<TConcreteOptions> where TConcreteOptions : class, IBaseRegistrationOptionsForExternalInstance<TConcreteOptions>
    {
        protected readonly HashSet<Type> AbstractionTypes = new HashSet<Type>();
        protected readonly IReadOnlyList<Type> IgnoredAbstractionTypes;
        protected readonly Type TargetType;
        protected readonly TypeInfo TargetTypeInfo;
        protected readonly TConcreteOptions This;

        protected BaseRegistrationOptionsForExternalInstance(Type targetType,
                                                             IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            targetType.MustNotBeNull(nameof(targetType));
            ignoredAbstractionTypes.MustNotBeNull(nameof(ignoredAbstractionTypes));

            TargetType = targetType;
            TargetTypeInfo = targetType.GetTypeInfo();
            IgnoredAbstractionTypes = ignoredAbstractionTypes;
            This = this as TConcreteOptions;
            EnsureThisIsNotNull();
        }

        public IEnumerable<Type> MappedAbstractionTypes => AbstractionTypes;

        public bool IsContainerTrackingDisposables { get; protected set; } = true;
        public string RegistrationName { get; protected set; }

        public TConcreteOptions UseRegistrationName(string registrationName)
        {
            RegistrationName = registrationName;
            return This;
        }

        public TConcreteOptions DisableIDisposableTrackingForThisType()
        {
            IsContainerTrackingDisposables = false;
            return This;
        }

        public TConcreteOptions MapToAllImplementedInterfaces()
        {
            return MapToAbstractions(TargetTypeInfo.ImplementedInterfaces);
        }

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

        public TConcreteOptions MapToAbstractions(params Type[] abstractionTypes)
        {
            return MapToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        public TConcreteOptions UseTypeNameAsRegistrationName()
        {
            RegistrationName = TargetType.Name;
            return This;
        }

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