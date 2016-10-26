using System;
using System.Diagnostics;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public static class GuardClausesExtensions
    {
        public static bool InheritsFromOrImplements(this Type parameter, Type baseType)
        {
            var baseTypeInfo = baseType.GetTypeInfo();
            var parameterTypeInfo = parameter.GetTypeInfo();
            if (baseTypeInfo.IsClass)
            {
                do
                {
                    if (parameterTypeInfo.BaseType.IsEquivalentTo(baseType))
                        return true;

                    parameterTypeInfo = parameterTypeInfo.BaseType.GetTypeInfo();
                } while (parameterTypeInfo.BaseType != null);
            }
            else if (baseTypeInfo.IsInterface)
            {
                foreach (var @interface in parameterTypeInfo.ImplementedInterfaces)
                {
                    if (@interface.IsEquivalentTo(baseType))
                        return true;
                }
            }

            return false;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustInheritFromOrImplement(this Type parameter, Type baseType)
        {
            if (parameter.InheritsFromOrImplements(baseType) == false)
                throw new TypeRegistrationException($"The concrete type \"{parameter}\" does not inherit from or implement type \"{baseType}\".", parameter);
        }

        private static bool IsEquivalentTo(this Type type, Type other)
        {
            if (type == other)
                return true;

            if (type.IsConstructedGenericType == false)
                return false;

            return type.GetGenericTypeDefinition() == other;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeClosedVariantOf(this Type type, Type genericTypeDefinition)
        {
            type.MustNotBeNull(nameof(type));

            var genericTypeDefinitionInfo = genericTypeDefinition.GetTypeInfo();
            if (genericTypeDefinitionInfo.IsGenericTypeDefinition == false)
                throw new InvalidOperationException($"The type \"{genericTypeDefinition}\" is not a generic type definition.");

            if (type.IsConstructedGenericType == false || type.GetGenericTypeDefinition() != genericTypeDefinition)
                throw new ResolveTypeException($"The type \"{type}\" is not a closed variant of the generic type definition \"{genericTypeDefinition}\".", type);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeRegistrationCompliant(this Type type, RegisterComplianceExceptions customExceptions = null)
        {
            var typeInfo = type.GetTypeInfo();
            customExceptions = customExceptions ?? RegisterComplianceExceptions.Default;

            if (typeInfo.IsInterface)
                throw customExceptions.CreateExceptionForInterfaceType(type);
            if (typeInfo.IsAbstract)
                throw customExceptions.CreateExceptionForAbstractType(type);
            if (typeInfo.IsGenericParameter)
                throw customExceptions.CreateExceptionForGenericParameterType(type);
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters && typeInfo.IsGenericTypeDefinition == false)
                throw customExceptions.CreateExceptionForOpenGenericType(type);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeResolveCompliant(this Type type, ResolveComplianceExceptions customExceptions = null)
        {
            customExceptions = customExceptions ?? ResolveComplianceExceptions.Default;
            type.MustBeRegistrationCompliant(customExceptions);

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericTypeDefinition)
                throw customExceptions.CreateExceptionForGenericTypeDefinition(type);
            if (typeInfo.IsEnum)
                throw customExceptions.CreateExceptionForEnumType(type);
            if (typeInfo.BaseType == typeof(MulticastDelegate))
                throw customExceptions.CreateExceptionForDelegateType(type);
        }
    }
}