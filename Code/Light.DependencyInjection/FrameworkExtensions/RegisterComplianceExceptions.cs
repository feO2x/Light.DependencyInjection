using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    /// <summary>
    ///     Represents a set of delegates that are called by <see cref="GuardClausesExtensions.MustBeRegistrationCompliant" />
    ///     when a type cannot be registered.
    /// </summary>
    public class RegisterComplianceExceptions
    {
        /// <summary>
        ///     Gets the default compliance exceptions for registration.
        /// </summary>
        public static readonly RegisterComplianceExceptions Default = new RegisterComplianceExceptions();

        private Func<Type, Exception> _createExceptionForAbstractType;
        private Func<Type, Exception> _createExceptionForGenericParameterType;
        private Func<Type, Exception> _createExceptionForInterfaceType;
        private Func<Type, Exception> _createExceptionForOpenGenericType;

        /// <summary>
        ///     Initializes a new instance of <see cref="RegisterComplianceExceptions" />
        /// </summary>
        public RegisterComplianceExceptions()
        {
            CreateExceptionForInterfaceType = CreateDefaultExceptionForInterfaceType;
            CreateExceptionForAbstractType = CreateDefaultExceptionForAbstractType;
            CreateExceptionForGenericParameterType = CreateDefaultExceptionForGenericParameterType;
            CreateExceptionForOpenGenericType = CreateDefaultExceptionForOpenGenericType;
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for interface types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForInterfaceType
        {
            get { return _createExceptionForInterfaceType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForInterfaceType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for abstract class types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForAbstractType
        {
            get { return _createExceptionForAbstractType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForAbstractType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for generic parameter types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForGenericParameterType
        {
            get { return _createExceptionForGenericParameterType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForGenericParameterType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that creates the exception for open generic types.
        /// </summary>
        public Func<Type, Exception> CreateExceptionForOpenGenericType
        {
            get { return _createExceptionForOpenGenericType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForOpenGenericType = value;
            }
        }

        /// <summary>
        ///     Creates the default <see cref="TypeRegistrationException"/> for interface types.
        /// </summary>
        public static Exception CreateDefaultExceptionForInterfaceType(Type interfaceType)
        {
            return new TypeRegistrationException($"You cannot register type \"{interfaceType}\" because it is an interface which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", interfaceType);
        }

        /// <summary>
        ///     Creates the default <see cref="TypeRegistrationException"/> for abstract class types.
        /// </summary>
        public static Exception CreateDefaultExceptionForAbstractType(Type abstractType)
        {
            return new TypeRegistrationException($"You cannot register type \"{abstractType}\" because it is an abstract class which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", abstractType);
        }

        /// <summary>
        ///     Creates the default <see cref="TypeRegistrationException"/> for generic parameter types.
        /// </summary>
        public static Exception CreateDefaultExceptionForGenericParameterType(Type genericParameterType)
        {
            return new TypeRegistrationException($"You cannot register type \"{genericParameterType}\" because it is a generic parameter type. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", genericParameterType);
        }

        /// <summary>
        ///     Creates the default <see cref="TypeRegistrationException"/> for open generic types.
        /// </summary>
        public static Exception CreateDefaultExceptionForOpenGenericType(Type openGenericType)
        {
            return new TypeRegistrationException($"You cannot register type \"{openGenericType}\" because it is a bound open generic type. Please ensure that you provide the generic type definition of this type.", openGenericType);
        }

        /// <summary>
        ///     Creates a shallow copy of this instance.
        /// </summary>
        public RegisterComplianceExceptions Clone()
        {
            return (RegisterComplianceExceptions) MemberwiseClone();
        }
    }
}