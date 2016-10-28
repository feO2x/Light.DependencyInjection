using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    /// <summary>
    ///     Represents a set of delegates creating exceptions, which are called by <see cref="GuardClausesExtensions.MustBeAutomaticResolveCompliant" />
    ///     when a type does not fulfill the requirements to be automatically resolved by the DI container.
    /// </summary>
    public sealed class AutomaticResolveComplianceExceptions : RegisterComplianceExceptions
    {
        /// <summary>
        ///     Gets the default compliance exceptions for automatic resolve.
        /// </summary>
        public new static readonly AutomaticResolveComplianceExceptions Default = new AutomaticResolveComplianceExceptions();

        private Func<Type, Exception> _createExceptionForDelegateType;
        private Func<Type, Exception> _createExceptionForEnumType;
        private Func<Type, Exception> _createExceptionForGenericTypeDefinition;
        private Func<Type, Exception> _createExceptionForPrimitiveType;

        /// <summary>
        ///     Initializes a new instance of <see cref="AutomaticResolveComplianceExceptions" />.
        /// </summary>
        public AutomaticResolveComplianceExceptions()
        {
            CreateExceptionForInterfaceType = CreateDefaultExceptionForInterfaceType;
            CreateExceptionForAbstractType = CreateDefaultExceptionForAbstractType;
            CreateExceptionForGenericParameterType = CreateDefaultExceptionForGenericParameterType;
            CreateExceptionForOpenGenericType = CreateDefaultExceptionForOpenGenericType;
            CreateExceptionForGenericTypeDefinition = CreateDefaultExceptionForGenericTypeDefinition;
            CreateExceptionForEnumType = CreateDefaultExceptionForEnumType;
            CreateExceptionForPrimitiveType = CreateDefaultExceptionForPrimitiveType;
            CreateExceptionForDelegateType = CreateDefaultExceptionForDelegateType;
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for generic type definitions.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForGenericTypeDefinition
        {
            get { return _createExceptionForGenericTypeDefinition; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForGenericTypeDefinition = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for enum types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForEnumType
        {
            get { return _createExceptionForEnumType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForEnumType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for delegate types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForDelegateType
        {
            get { return _createExceptionForDelegateType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForDelegateType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for primitive types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForPrimitiveType
        {
            get { return _createExceptionForPrimitiveType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForPrimitiveType = value;
            }
        }

        /// <summary>
        ///     Creates a shallow copy of this instance.
        /// </summary>
        public new AutomaticResolveComplianceExceptions Clone()
        {
            return (AutomaticResolveComplianceExceptions) MemberwiseClone();
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for interface types.
        /// </summary>
        public new static Exception CreateDefaultExceptionForInterfaceType(Type interfaceType)
        {
            return new ResolveTypeException($"The specified interface type \"{interfaceType}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.", interfaceType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for abstract class types.
        /// </summary>
        public new static Exception CreateDefaultExceptionForAbstractType(Type abstractType)
        {
            return new ResolveTypeException($"The specified abstract class \"{abstractType}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.", abstractType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for generic parameter types.
        /// </summary>
        public new static Exception CreateDefaultExceptionForGenericParameterType(Type genericParameterType)
        {
            return new ResolveTypeException($"The specified type \"{genericParameterType}\" is a generic parameter which cannot be resolved by the Dependency Injection Container.", genericParameterType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for open generic types.
        /// </summary>
        public new static Exception CreateDefaultExceptionForOpenGenericType(Type openGenericType)
        {
            return new ResolveTypeException($"The specified type \"{openGenericType}\" is an open generic type which cannot be resolved by the Dependency Injection Container.", openGenericType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for generic type definitions.
        /// </summary>
        public static Exception CreateDefaultExceptionForGenericTypeDefinition(Type genericTypeDefinition)
        {
            return new ResolveTypeException($"The specified type \"{genericTypeDefinition}\" is a generic type definition which cannot be resolved by the Dependency Injection Container.", genericTypeDefinition);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for enum types.
        /// </summary>
        public static Exception CreateDefaultExceptionForEnumType(Type enumType)
        {
            return new ResolveTypeException($"The specified type \"{enumType}\" describes an enum type which has not been registered and which cannot be resolved automatically.", enumType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for primitive types.
        /// </summary>
        public static Exception CreateDefaultExceptionForPrimitiveType(Type primitiveType)
        {
            return new ResolveTypeException($"The specified type \"{primitiveType}\" is a primitive type which cannot be automatically resolved by the Dependency Injection Container.", primitiveType);
        }

        /// <summary>
        ///     Creates the default <see cref="ResolveTypeException" /> for delegate types.
        /// </summary>
        public static Exception CreateDefaultExceptionForDelegateType(Type delegateType)
        {
            return new ResolveTypeException($"The specified type \"{delegateType}\" describes a delegate type which has not been registered and which cannot be resolved automatically.", delegateType);
        }
    }
}