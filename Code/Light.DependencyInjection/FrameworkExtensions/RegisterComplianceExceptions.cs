using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public class RegisterComplianceExceptions
    {
        public static readonly RegisterComplianceExceptions Default = new RegisterComplianceExceptions();

        public RegisterComplianceExceptions()
        {
            CreateExceptionForInterfaceType = CreateDefaultExceptionForInterfaceType;
            CreateExceptionForAbstractType = CreateDefaultExceptionForAbstractType;
            CreateExceptionForGenericParameterType = CreateDefaultExceptionForGenericParameterType;
            CreateExceptionForOpenGenericType = CreateDefaultExceptionForOpenGenericType;
        }

        private Func<Type, Exception> _createExceptionForAbstractType;
        private Func<Type, Exception> _createExceptionForGenericParameterType;
        private Func<Type, Exception> _createExceptionForInterfaceType;
        private Func<Type, Exception> _createExceptionForOpenGenericType;

        public Func<Type, Exception> CreateExceptionForInterfaceType
        {
            get { return _createExceptionForInterfaceType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForInterfaceType = value;
            }
        }

        public Func<Type, Exception> CreateExceptionForAbstractType
        {
            get { return _createExceptionForAbstractType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForAbstractType = value;
            }
        }

        public Func<Type, Exception> CreateExceptionForGenericParameterType
        {
            get { return _createExceptionForGenericParameterType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForGenericParameterType = value;
            }
        }

        public Func<Type, Exception> CreateExceptionForOpenGenericType
        {
            get { return _createExceptionForOpenGenericType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForOpenGenericType = value;
            }
        }


        public static Exception CreateDefaultExceptionForInterfaceType(Type interfaceType)
        {
            return new TypeRegistrationException($"You cannot register type \"{interfaceType}\" because it is an interface which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", interfaceType);
        }

        public static Exception CreateDefaultExceptionForAbstractType(Type abstractType)
        {
            return new TypeRegistrationException($"You cannot register type \"{abstractType}\" because it is an abstract class which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", abstractType);
        }

        public static Exception CreateDefaultExceptionForGenericParameterType(Type genericParameterType)
        {
            return new TypeRegistrationException($"You cannot register type \"{genericParameterType}\" because it is a generic parameter type. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.", genericParameterType);
        }

        public static Exception CreateDefaultExceptionForOpenGenericType(Type openGenericType)
        {
            return new TypeRegistrationException($"You cannot register type \"{openGenericType}\" because it is a bound open generic type. Please ensure that you provide the generic type definition of this type.", openGenericType);
        }

        public RegisterComplianceExceptions Clone()
        {
            return (RegisterComplianceExceptions) MemberwiseClone();
        }
    }
}