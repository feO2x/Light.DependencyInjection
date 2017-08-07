using System;
using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public abstract class BaseInstantiationExpressionFactory<T> : IInstantiationExpressionFactory where T : InstantiationInfo
    {
        public Type InstantiationInfoType { get; } = typeof(T);

        public Expression Create(InstantiationInfo instantiationInfo, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            return Create(instantiationInfo.MustBeOfType<T>(nameof(instantiationInfo)), context, parameterExpressions);
        }

        protected abstract Expression Create(T instantiationInfo, ResolveExpressionContext context, Expression[] parameterExpressions);
    }
}