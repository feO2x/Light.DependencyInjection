using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IInstantiationExpressionFactory
    {
        Type InstantiationInfoType { get; }

        Expression Create(InstantiationInfo instantiationInfo, Expression[] parameterExpressions);
    }
}