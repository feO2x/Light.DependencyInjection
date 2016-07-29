using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public static class ReflectionExtensions
    {
        // TODO: we should also allow static methods here, too
        public static Func<object[], object> CompileObjectCreationFunction(this ConstructorInfo constructorInfo)
        {
            constructorInfo.MustNotBeNull(nameof(constructorInfo));

            var constructorParameterInfos = constructorInfo.GetParameters();
            // TODO: check if it is the default constructor

            var parameterExpression = Expression.Parameter(typeof(object[]));

            var argumentExpressions = new List<Expression>();
            for (var i = 0; i < constructorParameterInfos.Length; i++)
            {
                var castExpression = Expression.Convert(Expression.ArrayIndex(parameterExpression, Expression.Constant(i)), constructorParameterInfos[i].ParameterType);
                argumentExpressions.Add(castExpression);
            }

            var newExpression = Expression.New(constructorInfo, argumentExpressions);
            return Expression.Lambda<Func<object[], object>>(newExpression, parameterExpression).Compile();
        }
    }
}