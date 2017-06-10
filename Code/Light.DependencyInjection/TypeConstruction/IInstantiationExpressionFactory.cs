using System;
using System.Linq.Expressions;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IInstantiationExpressionFactory
    {
        Type InstantiationInfoType { get; }

        Expression Create(InstantiationInfo instantiationInfo, Expression[] parameterExpressions);
    }
}