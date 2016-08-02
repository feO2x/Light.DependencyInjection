using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public static class GuardClausesExtensions
    {
        [Conditional(Check.CompileAssertionsSymbol)]
        public static void MustInheritFromOrImplement(this Type parameter, Type baseType, string parameterName = null, string message = null, Func<Exception> exception = null)
        {
            parameter.MustNotBeNull();
            baseType.MustNotBeNull();

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

            throw new TypeRegistrationException($"The concrete type \"{parameter}\" does not inherit from or implement type \"{baseType}\"", parameter);
        }
    }
}