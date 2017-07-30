using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class DelegateInstantiationExpressionFactory : BaseInstantiationExpressionFactory<DelegateInstantiationInfo>
    {
        protected override Expression Create(DelegateInstantiationInfo instantiationInfo, Expression[] parameterExpressions)
        {
            var methodInfo = instantiationInfo.Delegate.GetMethodInfo();
            return Expression.Call(methodInfo, parameterExpressions);
        }
    }
}