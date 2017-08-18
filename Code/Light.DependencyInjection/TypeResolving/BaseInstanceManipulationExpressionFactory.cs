using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public abstract class BaseInstanceManipulationExpressionFactory<T> : IInstanceManipulationExpressionFactory where T : InstanceManipulation
    {
        public Type InstanceManipulationType { get; } = typeof(T);

        public Expression Create(InstanceManipulation instanceManipulation, ParameterExpression instanceVariableExpression, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            return Create(instanceManipulation.MustBeOfType<T>(nameof(instanceManipulation)), instanceVariableExpression.MustNotBeNull(nameof(instanceVariableExpression)), context, parameterExpressions);
        }

        protected abstract Expression Create(T instanceManipulation, ParameterExpression instanceVariableExpression, ResolveExpressionContext context, Expression[] parameterExpressions);
    }
}