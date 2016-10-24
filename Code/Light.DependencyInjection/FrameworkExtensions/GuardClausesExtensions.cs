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
        public static void MustBeGenericTypeDefinition(this TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericTypeDefinition)
                return;

            throw new TypeRegistrationException($"The type \"{typeInfo.FullName}\" is no generic type definition.", typeInfo.AsType());
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeNonGenericOrClosedConstructedOrGenericTypeDefinition(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType)
            {
                if (typeInfo.IsGenericTypeDefinition)
                    return;

                if (typeInfo.ContainsGenericParameters == false)
                    return;

                throw new TypeRegistrationException($"The type \"{type}\" cannot be registered with the DI container because it is an open constructed generic type. Only non-generic types, closed constructed generic types or generic type definitions can be registered.", type);
            }
            if (typeInfo.IsGenericParameter)
                throw new TypeRegistrationException($"The type \"{type}\" cannot be registered with the DI container because it is a generic type paramter. Only non-generic types, closed constructed generic types or generic type definitions can be registered.", type);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustBeInstantiatable(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            // TODO: check if type is a generic type definition, an open generic type, a delegate or something else that we do not support
            if (typeInfo.IsInterface)
                throw new TypeRegistrationException($"The type \"{type}\" cannot be registered with the DI container because it is an interface type that cannot be instantiated.", type);

            if (typeInfo.IsAbstract)
                throw new TypeRegistrationException($"The type \"{type}\" cannot be registered with the DI container because it is an abstract class that cannot be instantiated.", type);
        }
    }
}