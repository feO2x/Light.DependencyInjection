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

        public static ResolveDelegate CompileToResolveDelegate(this Expression expression, ParameterExpression resolveContextExpression)
        {
            expression = expression.MustNotBeNull(nameof(expression)).AdjustTypeToObjectIfNecessary();
            return Expression.Lambda<ResolveDelegate>(expression, resolveContextExpression).Compile();
        }
    }
}