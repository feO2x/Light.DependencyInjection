using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class FieldInjectionExpressionFactory : BaseInstanceManipulationExpressionFactory<FieldInjection>
    {
        protected override Expression Create(FieldInjection fieldInjection, ParameterExpression instanceVariableExpression, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            parameterExpressions.MustHaveCount(1, nameof(parameterExpressions));
            return Expression.Assign(Expression.Field(instanceVariableExpression, fieldInjection.TargetField), parameterExpressions[0]);
        }
    }
}