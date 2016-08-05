using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection
{
    public static class ReflectionExtensions
    {
        // TODO: we should also allow static methods here, too
        public static Func<object[], object> CompileObjectCreationFunction(this ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            var constructorParameterInfos = constructorInfo.GetParameters();

            var parameterExpression = Expression.Parameter(typeof(object[]));
            if (constructorParameterInfos.Length == 0)
                return Expression.Lambda<Func<object[], object>>(Expression.New(constructorInfo), parameterExpression).Compile();

            var argumentExpressions = new List<Expression>();
            for (var i = 0; i < constructorParameterInfos.Length; i++)
            {
                var castExpression = Expression.Convert(Expression.ArrayIndex(parameterExpression, Expression.Constant(i)), constructorParameterInfos[i].ParameterType);
                argumentExpressions.Add(castExpression);
            }

            var newExpression = Expression.New(constructorInfo, argumentExpressions);
            return Expression.Lambda<Func<object[], object>>(newExpression, parameterExpression).Compile();
        }

        public static ConstructorInfo FindDefaultConstructor(this IEnumerable<ConstructorInfo> constructors)
        {
            var constructorList = constructors.AsList();

            if (constructorList.Count == 0)
                return null;

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructor = constructorList[i];
                if (constructor.GetParameters().Length == 0)
                    return constructor;
            }

            return null;
        }

        public static ConstructorInfo FindConstructorWithArgumentTypes(this IEnumerable<ConstructorInfo> constructors, params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var constructorList = constructors.AsList();
            if (constructorList.Count == 0)
                return null;

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructorInfo = constructorList[i];
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length != parameterTypes.Length)
                    continue;

                if (CheckParameterEquality(parameterInfos, parameterTypes))
                    return constructorInfo;
            }

            return null;
        }

        private static bool CheckParameterEquality(ParameterInfo[] parameterInfos, Type[] parameterTypes)
        {
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                if (parameterInfos[i].ParameterType != parameterTypes[i])
                    return false;
            }

            return true;
        }
    }
}