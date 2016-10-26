using System;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public sealed class ResolveComplianceExceptions : RegisterComplianceExceptions
    {
        public new static readonly ResolveComplianceExceptions Default = new ResolveComplianceExceptions();
        private Func<Type, Exception> _createExceptionForDelegateType;
        private Func<Type, Exception> _createExceptionForEnumType;

        private Func<Type, Exception> _createExceptionForGenericTypeDefinition;

        public ResolveComplianceExceptions()
        {
            CreateExceptionForInterfaceType = CreateDefaultExceptionForInterfaceType;
            CreateExceptionForAbstractType = CreateDefaultExceptionForAbstractType;
            CreateExceptionForGenericParameterType = CreateDefaultExceptionForGenericParameterType;
            CreateExceptionForOpenGenericType = CreateDefaultExceptionForOpenGenericType;
            CreateExceptionForGenericTypeDefinition = CreateDefaultExceptionForGenericTypeDefinition;
            CreateExceptionForEnumType = CreateDefaultExceptionForEnumType;
            CreateExceptionForDelegateType = CreateDefaultExceptionForDelegateType;
        }

        public Func<Type, Exception> CreateExceptionForGenericTypeDefinition
        {
            get { return _createExceptionForGenericTypeDefinition; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForGenericTypeDefinition = value;
            }
        }

        public Func<Type, Exception> CreateExceptionForEnumType
        {
            get { return _createExceptionForEnumType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForEnumType = value;
            }
        }

        public Func<Type, Exception> CreateExceptionForDelegateType
        {
            get { return _createExceptionForDelegateType; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _createExceptionForDelegateType = value;
            }
        }

        public new ResolveComplianceExceptions Clone()
        {
            return (ResolveComplianceExceptions) MemberwiseClone();
        }

        public new static Exception CreateDefaultExceptionForInterfaceType(Type interfaceType)
        {
            return new ResolveTypeException($"The specified interface type \"{interfaceType}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.", interfaceType);
        }

        public new static Exception CreateDefaultExceptionForAbstractType(Type abstractType)
        {
            return new ResolveTypeException($"The specified abstract class \"{abstractType}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.", abstractType);
        }

        public new static Exception CreateDefaultExceptionForGenericParameterType(Type genericParameterType)
        {
            return new ResolveTypeException($"The specified type \"{genericParameterType}\" is a generic parameter which cannot be resolved by the Dependency Injection Container.", genericParameterType);
        }

        public new static Exception CreateDefaultExceptionForOpenGenericType(Type openGenericType)
        {
            return new ResolveTypeException($"The specified type \"{openGenericType}\" is an open generic type which cannot be resolved by the Dependency Injection Container.", openGenericType);
        }

        public static Exception CreateDefaultExceptionForGenericTypeDefinition(Type genericTypeDefinition)
        {
            return new ResolveTypeException($"The specified type \"{genericTypeDefinition}\" is a generic type definition which cannot be resolved by the Dependency Injection Container.", genericTypeDefinition);
        }

        public static Exception CreateDefaultExceptionForEnumType(Type enumType)
        {
            return new ResolveTypeException($"The specified type \"{enumType}\" describes an enum type which has not been registered and which cannot be resolved automatically.", enumType);
        }

        public static Exception CreateDefaultExceptionForDelegateType(Type delegateType)
        {
            return new ResolveTypeException($"The specified type \"{delegateType}\" describes a delegate type which has not been registered and which cannot be resolved automatically.", delegateType);
        }
    }
}