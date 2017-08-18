using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class PropertyInjectionExpressionFactory : BaseInstanceManipulationExpressionFactory<PropertyInjection>
    {
        protected override Expression Create(PropertyInjection propertyInjection, ParameterExpression instanceVariableExpression, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            var targetProperty = propertyInjection.TargetProperty;
            if (context.IsResolvingGenericTypeDefinition)
                targetProperty = context.ResolvedGenericRegistrationType.GetRuntimeProperty(propertyInjection.TargetProperty.Name);
            return Expression.Call(instanceVariableExpression, targetProperty.SetMethod, parameterExpressions);
        }
    }
}