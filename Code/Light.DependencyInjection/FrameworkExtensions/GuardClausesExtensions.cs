﻿using System;
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
        public static void MustBeClosedConstructedVariantOf(this Type type, Type genericTypeDefinition)
        {
            type.MustNotBeNull(nameof(type));

            var genericTypeDefinitionInfo = genericTypeDefinition.GetTypeInfo();
            if (genericTypeDefinitionInfo.IsGenericTypeDefinition == false)
                throw new ArgumentException($"The type \"{genericTypeDefinition}\" is no generic type definition.");

            if (type.IsConstructedGenericType == false || type.GetGenericTypeDefinition() != genericTypeDefinition)
                throw new ResolveTypeException($"The type \"{type}\" is closed constructed variant of the generic type definition \"{genericTypeDefinition}\".", type);
        }

        public static bool IsNonGenericOrClosedConstructedOrGenericTypeDefinition(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType == false)
                return true;
            if (typeInfo.IsGenericTypeDefinition)
                return true;
            return !typeInfo.ContainsGenericParameters;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeNonGenericOrClosedConstructedOrGenericTypeDefinition(this Type type)
        {
            if (type.IsNonGenericOrClosedConstructedOrGenericTypeDefinition())
                return;

            throw new TypeRegistrationException($"The type \"{type}\" cannot be registered with the DI container because it is an open constructed generic type. Only non-generic types, closed constructed generic types or generic type definitions can be registered.", type);
        }
    }
}