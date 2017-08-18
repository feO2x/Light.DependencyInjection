using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public abstract class CommonRegistrationOptions<TOptions> : ICommonRegistrationOptions<TOptions> where TOptions : class, ICommonRegistrationOptions<TOptions>
    {
        protected readonly IReadOnlyList<Type> IgnoredAbstractionTypes;
        protected readonly HashSet<Type> MappedAbstractionTypes = new HashSet<Type>();
        protected readonly Type TargetType;
        protected readonly TypeInfo TargetTypeInfo;
        protected readonly TOptions This;
        private string _registrationName = string.Empty;
        protected bool IsTrackingDisposables = true;

        protected CommonRegistrationOptions(Type targetType, IReadOnlyList<Type> ignoredAbstractionTypes)
        {
            TargetType = targetType.MustBeValidRegistrationType();
            TargetTypeInfo = TargetType.GetTypeInfo();
            IgnoredAbstractionTypes = ignoredAbstractionTypes.MustNotBeNull();
            This = (this as TOptions).MustNotBeNull(exception: () => new InvalidOperationException("You did not derive correctly from the base options type."));
        }

        protected string RegistrationName
        {
            get => _registrationName;
            set => _registrationName = value.MustNotBeNull();
        }

        public TOptions UseRegistrationName(string registrationName)
        {
            RegistrationName = registrationName;
            return This;
        }

        public TOptions UseTypeNameAsRegistrationName()
        {
            _registrationName = TargetType.Name;
            return This;
        }

        public TOptions UseFullTypeNameAsRegistrationName()
        {
            _registrationName = TargetType.FullName;
            return This;
        }

        public TOptions DisableIDisposableTracking()
        {
            IsTrackingDisposables = false;
            return This;
        }

        public TOptions MapToAbstractions(params Type[] abstractionTypes)
        {
            return MapToAbstractions((IEnumerable<Type>) abstractionTypes);
        }

        public TOptions MapToAbstractions(IEnumerable<Type> abstractionTypes)
        {
            // ReSharper disable PossibleMultipleEnumeration
            abstractionTypes.MustNotContainNull(nameof(abstractionTypes));

            foreach (var abstractionType in abstractionTypes)
            {
                MappedAbstractionTypes.Add(abstractionType);
            }
            // ReSharper restore PossibleMultipleEnumeration
            return This;
        }

        public TOptions MapToAllImplementedInterfaces()
        {
            foreach (var @interface in TargetTypeInfo.ImplementedInterfaces)
            {
                if (IgnoredAbstractionTypes.Contains(@interface) == false)
                    MappedAbstractionTypes.Add(@interface);
            }
            return This;
        }

        public TOptions MapToBaseClass()
        {
            if (TargetTypeInfo.BaseType == null)
                throw new InvalidOperationException($"Type \"{TargetType}\" does not have a base class.");

            MappedAbstractionTypes.Add(TargetTypeInfo.BaseType);
            return This;
        }
    }
}