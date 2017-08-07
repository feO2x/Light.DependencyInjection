using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ConstructorInstantiationExpressionFactory : BaseInstantiationExpressionFactory<ConstructorInstantiationInfo>
    {
        protected override Expression Create(ConstructorInstantiationInfo instantiationInfo,
                                             ResolveExpressionContext context,
                                             Expression[] parameterExpressions)
        {
            if (instantiationInfo.TypeKey.Type.IsGenericTypeDefinition() == false)
                return Expression.New(instantiationInfo.ConstructorInfo, parameterExpressions);

            var targetConstructor = FindConstructorOfClosedConstructedGenericType(context, instantiationInfo);
            return Expression.New(targetConstructor, parameterExpressions);
        }

        private static ConstructorInfo FindConstructorOfClosedConstructedGenericType(ResolveExpressionContext context, ConstructorInstantiationInfo instantiationInfo)
        {
            var constructors = context.ResolvedGenericRegistrationTypeInfo.DeclaredConstructors.AsReadOnlyList();
            var parametersOfGenericTypeDefinition = instantiationInfo.ConstructorInfo.GetParameters();
            var numberOfConstructorParameters = parametersOfGenericTypeDefinition.Length;
            for (var i = 0; i < constructors.Count; i++)
            {
                var targetConstructor = constructors[i];
                var parameters = targetConstructor.GetParameters();
                if (parameters.Length != numberOfConstructorParameters)
                    continue;

                if (MatchConstructorParameters(parameters, parametersOfGenericTypeDefinition, context))
                    return targetConstructor;
            }
            throw new ResolveException($"The constructor for the closed constructed generic type \"{context.RequestedType}\" that matches the generic type definition's constructor \"{instantiationInfo.ConstructorInfo}\" could not be found. This exception should actually never happen, unless there is a bug in class \"{nameof(ConstructorInstantiationInfoFactory)}\" or you messed with the .NET type system badly.");
        }

        private static bool MatchConstructorParameters(ParameterInfo[] constructedGenericTypeParameters,
                                                       ParameterInfo[] genericTypeDefinitionParameters,
                                                       ResolveExpressionContext context)
        {
            for (var i = 0; i < constructedGenericTypeParameters.Length; i++)
            {
                var firstParameter = constructedGenericTypeParameters[i];
                var secondParameter = genericTypeDefinitionParameters[i];
                var resolvedParameterType = secondParameter.ParameterType.IsGenericParameter
                                                ? context.ResolveGenericTypeParameter(secondParameter.ParameterType)
                                                : secondParameter.ParameterType;

                if (resolvedParameterType != firstParameter.ParameterType)
                    return false;
            }
            return true;
        }
    }
}