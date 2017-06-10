using System.Linq.Expressions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ConstructorInstantiationExpressionFactory : BaseInstantiationExpressionFactory<ConstructorInstantiationInfo>
    {
        protected override Expression Create(ConstructorInstantiationInfo instantiationInfo, Expression[] parameterExpressions)
        {
            return Expression.New(instantiationInfo.ConstructorInfo, parameterExpressions);
        }
    }
}