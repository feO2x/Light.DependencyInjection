using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IInstanceManipulationExpressionFactory
    {
        Type InstanceManipulationType { get; }

        Expression Create(InstanceManipulation instanceManipulation, ParameterExpression instanceVariableExpression, Expression[] parameterExpressions);
    }
}