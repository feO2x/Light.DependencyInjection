using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.FrameworkExtensions
{
    public static class GuardClausesExtensions
    {
        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustInheritFromOrImplement(this Type parameter, Type baseType)
        {
            parameter.MustNotBeNull(nameof(parameter));
            baseType.MustNotBeNull(nameof(baseType));

            var baseTypeInfo = baseType.GetTypeInfo();
            var parameterTypeInfo = parameter.GetTypeInfo();
            if (baseTypeInfo.IsClass)
            {
                do
                {
                    if (parameterTypeInfo.BaseType == baseType)
                        return;

                    parameterTypeInfo = parameterTypeInfo.BaseType.GetTypeInfo();
                } while (parameterTypeInfo.BaseType != null);
            }
            else if (baseTypeInfo.IsInterface && parameterTypeInfo.ImplementedInterfaces.Contains(baseType))
                return;

            throw new TypeRegistrationException($"The concrete type \"{parameter}\" does not inherit from or implement type \"{baseType}\".", parameter);
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
    }
}