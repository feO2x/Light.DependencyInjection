using System;
using System.Linq.Expressions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class BaseInstantiationExpressionFactory<T> : IInstantiationExpressionFactory where T : InstantiationInfo
    {
        public Type InstantiationInfoType { get; } = typeof(T);

        public Expression Create(InstantiationInfo instantiationInfo, Expression[] parameterExpressions)
        {
            return Create(instantiationInfo.MustBeOfType<T>(), parameterExpressions);
        }

        protected abstract Expression Create(T instantiationInfo, Expression[] parameterExpressions);
    }
}