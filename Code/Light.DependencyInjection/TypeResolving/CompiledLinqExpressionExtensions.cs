using System;
using System.Linq.Expressions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public static class CompiledLinqExpressionExtensions
    {
        public static Expression AdjustTypeToObjectIfNecessary(this Expression expression)
        {
            return expression.MustNotBeNull(nameof(expression)).Type == KnownTypes.ObjectType ? expression : Expression.Convert(expression, KnownTypes.ObjectType);
        }

        public static Func<DiContainer, object> CompileToResolveDelegate(this Expression expression, ParameterExpression diContainerExpression)
        {
            expression = expression.MustNotBeNull(nameof(expression)).AdjustTypeToObjectIfNecessary();
            return Expression.Lambda<Func<DiContainer, object>>(expression, diContainerExpression).Compile();
        }
    }
}