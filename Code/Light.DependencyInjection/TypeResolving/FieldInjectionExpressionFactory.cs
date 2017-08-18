using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class FieldInjectionExpressionFactory : BaseInstanceManipulationExpressionFactory<FieldInjection>
    {
        protected override Expression Create(FieldInjection fieldInjection, ParameterExpression instanceVariableExpression, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            parameterExpressions.MustHaveCount(1, nameof(parameterExpressions));

            var targetField = fieldInjection.TargetField;
            if (context.IsResolvingGenericTypeDefinition)
                targetField = context.ResolvedGenericRegistrationType.GetRuntimeField(targetField.Name);

            return Expression.Assign(Expression.Field(instanceVariableExpression, targetField), parameterExpressions[0]);
        }
    }
}