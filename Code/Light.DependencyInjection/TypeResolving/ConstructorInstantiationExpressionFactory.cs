using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ConstructorInstantiationExpressionFactory : BaseInstantiationExpressionFactory<ConstructorInstantiationInfo>
    {
        protected override Expression Create(ConstructorInstantiationInfo instantiationInfo,
                                             ResolveExpressionContext context,
                                             Expression[] parameterExpressions)
        {
            if (context.IsResolvingGenericTypeDefinition == false)
                return Expression.New(instantiationInfo.ConstructorInfo, parameterExpressions);

            var targetConstructor = FindConstructorOfClosedConstructedGenericType(context, instantiationInfo);
            return Expression.New(targetConstructor, parameterExpressions);
        }

        private static ConstructorInfo FindConstructorOfClosedConstructedGenericType(ResolveExpressionContext context, ConstructorInstantiationInfo instantiationInfo)
        {
            var constructors = context.ResolvedGenericRegistrationTypeInfo.DeclaredConstructors.AsReadOnlyList();
            var targetConstructor = context.FindResolvedGenericMethod(instantiationInfo.ConstructorInfo, constructors);
            if (targetConstructor != null)
                return targetConstructor;

            throw new ResolveException($"The constructor for the closed constructed generic type \"{context.RequestedType}\" that matches the generic type definition's constructor \"{instantiationInfo.ConstructorInfo}\" could not be found. This exception should actually never happen, unless there is a bug in class \"{nameof(ConstructorInstantiationInfoFactory)}\" or you messed with the .NET type system badly.");
        }
    }
}