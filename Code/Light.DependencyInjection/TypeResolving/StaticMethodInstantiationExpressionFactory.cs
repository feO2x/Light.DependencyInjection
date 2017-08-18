using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class StaticMethodInstantiationExpressionFactory : BaseInstantiationExpressionFactory<StaticMethodInstantiationInfo>
    {
        protected override Expression Create(StaticMethodInstantiationInfo instantiationInfo, ResolveExpressionContext context, Expression[] parameterExpressions)
        {
            if (instantiationInfo.TypeKey.Type.IsGenericTypeDefinition() == false)
                return Expression.Call(instantiationInfo.StaticMethod, parameterExpressions);

            if (instantiationInfo.StaticMethod.DeclaringType.IsGenericTypeDefinition())
            {
                var closedConstructedStaticType = instantiationInfo.StaticMethod.DeclaringType.MakeGenericType(context.ResolvedGenericRegistrationTypeInfo.GenericTypeArguments);
                var closedConstructedMethods = closedConstructedStaticType.GetTypeInfo().GetDeclaredMethods(instantiationInfo.StaticMethod.Name).AsReadOnlyList();
                var staticMethodOfClosedConstructedStaticType = context.FindResolvedGenericMethod(instantiationInfo.StaticMethod, closedConstructedMethods);
                if (staticMethodOfClosedConstructedStaticType != null)
                    return Expression.Call(staticMethodOfClosedConstructedStaticType, parameterExpressions);
                throw new ResolveException($"The static method \"{instantiationInfo.StaticMethod}\" could not be found for the closed constructed type \"{closedConstructedStaticType}\". This exception should actually never happen, unless there is a bug in class \"{nameof(StaticMethodInstantiationInfoFactory)}\" or you messed with the .NET type system badly.");
            }

            var closedConstructedGenericMethod = instantiationInfo.StaticMethod.MakeGenericMethod(context.ResolvedGenericRegistrationTypeInfo.GenericTypeArguments);
            return Expression.Call(closedConstructedGenericMethod, parameterExpressions);
        }
    }
}