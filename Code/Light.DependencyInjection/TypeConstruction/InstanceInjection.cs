using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents a standardized set of information allowing the injection of single values into members after the declaring type was instantiated.
    /// </summary>
    public abstract class InstanceInjection : ISetTargetRegistrationName
    {
        /// <summary>
        ///     Gets the type that declares the target member.
        /// </summary>
        public readonly Type DeclaringType;

        /// <summary>
        ///     Gets the string representation of this instance injection.
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        ///     Gets the name of the target member.
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        ///     Gets the type of the target member.
        /// </summary>
        public readonly Type MemberType;

        /// <summary>
        ///     Gets the Standardized Set Value Function.
        /// </summary>
        public readonly Action<object, object> StandardizedSetValueFunction;

        private IDependencyResolver _dependencyResolver = DefaultDependencyResolver.Instance;

        /// <summary>
        ///     Gets or sets the registration name of the child value resolved for the target member.
        /// </summary>
        public string TargetRegistrationName;

        /// <summary>
        ///     Initializes a new instance of <see cref="InstanceInjection" />.
        /// </summary>
        /// <param name="memberName">The name of the target member.</param>
        /// <param name="memberType">The type of the target member.</param>
        /// <param name="declaringType">the type that declares the target member.</param>
        /// <param name="standardizedSetValueFunction">The Standardized Set Value Function which actually sets a resolved value on the target instance. Can be null if <paramref name="declaringType" /> is a generic type definition.</param>
        /// <param name="targetRegistrationName">The registration name of the child value resolved for the target member (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberName" />, <paramref name="memberType" />, or <paramref name="declaringType" /> is null.</exception>
        /// <exception cref="EmptyStringException">Thrown when <paramref name="memberName" /> is an empty string.</exception>
        /// <exception cref="StringIsOnlyWhiteSpaceException">Thrown when <paramref name="memberName" /> is only whitespace.</exception>
        protected InstanceInjection(string memberName,
                                    Type memberType,
                                    Type declaringType,
                                    Action<object, object> standardizedSetValueFunction,
                                    string targetRegistrationName = null)
        {
            memberName.MustNotBeNullOrWhiteSpace(nameof(memberName));
            memberType.MustNotBeNull(nameof(memberType));
            declaringType.MustNotBeNull(nameof(declaringType));

            MemberName = memberName;
            MemberType = memberType;
            DeclaringType = declaringType;
            StandardizedSetValueFunction = standardizedSetValueFunction;
            TargetRegistrationName = targetRegistrationName;
            DisplayName = $"{GetType().Name} {DeclaringType.Name}.{MemberName}";
        }

        /// <summary>
        ///     Gets or sets the dependency resolver being used to retrieve the child value that will be injected into the target member.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _dependencyResolver = value;
            }
        }

        string ISetTargetRegistrationName.TargetRegistrationName
        {
            set { TargetRegistrationName = value; }
        }

        /// <summary>
        ///     Resolves and injects the value into the target member of the specified instance, using the resolve context.
        /// </summary>
        public void InjectValue(object instance, ResolveContext context)
        {
            object value;
            if (context.ParameterOverrides.HasValue)
            {
                if (context.ParameterOverrides.Value.InstanceInjectionOverrides.TryGetValue(this, out value))
                    goto SetValue;
            }
            value = _dependencyResolver.Resolve(MemberType, TargetRegistrationName, context);

            SetValue:
            StandardizedSetValueFunction(instance, value);
        }


        /// <summary>
        ///     Returns the string representation of this instance injection.
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        ///     Creates a new <see cref="InstanceInjection" /> for the <paramref name="closedGenericType" /> with the
        ///     settings copied from this instance. You may only call this method if the declaring type is a generic type definition.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="closedGenericType" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the target type is not a generic type definition.</exception>
        /// <exception cref="ResolveTypeException">Thrown when <paramref name="closedGenericType" />is not a closed generic type variant of the target type.</exception>
        public InstanceInjection BindToClosedGenericType(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            closedGenericType.MustBeClosedVariantOf(DeclaringType);

            return BindToClosedGenericTypeInternal(closedGenericType, closedGenericTypeInfo);
        }

        /// <summary>
        ///     Creates a new instance of the derived class.
        /// </summary>
        protected abstract InstanceInjection BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo);
    }
}