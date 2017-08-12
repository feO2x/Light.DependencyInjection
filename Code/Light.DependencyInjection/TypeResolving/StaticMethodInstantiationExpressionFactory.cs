using System.Linq.Expressions;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class StaticMethodInstantiationExpressionFactory : BaseInstantiationExpressionFactory<StaticMethodInstantiationInfo>
    {
        protected override Expression Create(StaticMethodInstantiationInfo instantiationInfo, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            if (instantiationInfo.TypeKey.Type.IsGenericTypeDefinition() == false)
                return Expression.Call(instantiationInfo.StaticMethod, parameterExpressions);

            var closedConstructedGenericMethod = instantiationInfo.StaticMethod.MakeGenericMethod(context.ResolvedGenericRegistrationTypeInfo.GenericTypeArguments);
            return Expression.Call(closedConstructedGenericMethod, parameterExpressions);
        }
    }
}