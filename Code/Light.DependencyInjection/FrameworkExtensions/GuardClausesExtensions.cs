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
        public static void MustBeBoundVersionOfUnboundGenericType(this Type type, Type unboundgenericType)
        {
            type.MustNotBeNull(nameof(type));
            var unboundGenericTypeInfo = unboundgenericType.GetTypeInfo();
            if (unboundGenericTypeInfo.IsGenericTypeDefinition == false)
                throw new ArgumentException($"The type \"{unboundgenericType}\" is no unbound generic type.");

            if (type.IsConstructedGenericType == false || type.GetGenericTypeDefinition() != unboundgenericType)
                throw new ResolveTypeException($"The type \"{type}\" is no bound variant of the unbound generic type \"{unboundgenericType}\".", type);
        }
    }
}