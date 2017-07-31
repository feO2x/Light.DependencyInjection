using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class PropertyInjectionExpressionFactory : BaseInstanceManipulationExpressionFactory<PropertyInjection>
    {
        protected override Expression Create(PropertyInjection propertyInjection, ParameterExpression instanceVariableExpression, Expression[] parameterExpressions)
        {
            return Expression.Call(instanceVariableExpression, propertyInjection.TargetProperty.SetMethod, parameterExpressions);
        }
    }
}