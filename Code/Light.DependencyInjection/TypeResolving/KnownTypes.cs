using System;
using System.Linq.Expressions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public static class KnownTypes
    {
        public static readonly Type ObjectType = typeof(object);

        public static Expression AdjustTypeToObjectIfNecessary(this Expression expression)
        {
            return expression.MustNotBeNull(nameof(expression)).Type == ObjectType ? expression : Expression.Convert(expression, ObjectType);
        }

        public static Func<object> CompileToFuncOfObject(this Expression expression)
        {
            expression = expression.MustNotBeNull(nameof(expression)).AdjustTypeToObjectIfNecessary();
            return Expression.Lambda<Func<object>>(expression).Compile();
        }
    }
}