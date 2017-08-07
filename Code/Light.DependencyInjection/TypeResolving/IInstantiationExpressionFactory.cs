using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IInstantiationExpressionFactory
    {
        Type InstantiationInfoType { get; }

        Expression Create(InstantiationInfo instantiationInfo, ResolveExpressionContext context, Expression[] parameterExpressions);
    }
}