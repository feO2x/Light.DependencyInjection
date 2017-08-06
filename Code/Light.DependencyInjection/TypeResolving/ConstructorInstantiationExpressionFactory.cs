using System;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeResolving
{
    public sealed class ConstructorInstantiationExpressionFactory : BaseInstantiationExpressionFactory<ConstructorInstantiationInfo>
    {
        protected override Expression Create(ConstructorInstantiationInfo instantiationInfo, Type requestedType, Expression[] parameterExpressions)
        {
            if (instantiationInfo.TypeKey.Type.IsGenericTypeDefinition() == false)
                return Expression.New(instantiationInfo.ConstructorInfo, parameterExpressions);

            var closedConstructedGenericType = instantiationInfo.TypeKey.Type.MakeGenericType(requestedType.GenericTypeArguments);
            var targetConstructor = FindConstructorOfClosedConstructedGenericType(closedConstructedGenericType, instantiationInfo);
            return Expression.New(targetConstructor, parameterExpressions);
        }

        private static ConstructorInfo FindConstructorOfClosedConstructedGenericType(Type closedConstructedGenericType, ConstructorInstantiationInfo instantiationInfo)
        {
            var constructedTypeInfo = closedConstructedGenericType.GetTypeInfo();
            var constructors = constructedTypeInfo.DeclaredConstructors.AsReadOnlyList();
            var parametersOfGenericTypeDefinition = instantiationInfo.ConstructorInfo.GetParameters();
            var genericTypeDefinitionInfo = instantiationInfo.TypeKey.Type.GetTypeInfo();
            var numberOfConstructorParameters = parametersOfGenericTypeDefinition.Length;
            for (var i = 0; i < constructors.Count; i++)
            {
                var targetConstructor = constructors[i];
                var parameters = targetConstructor.GetParameters();
                if (parameters.Length != numberOfConstructorParameters)
                    continue;

                if (MatchConstructorParameters(parameters, parametersOfGenericTypeDefinition, constructedTypeInfo, genericTypeDefinitionInfo))
                    return targetConstructor;
            }
            throw new ResolveException($"The constructor for the closed constructed generic type \"{constructedTypeInfo}\" that matches the generic type definition's constructor \"{instantiationInfo}\" could not be found. This exception should actually never happen, unless there is a bug in class \"{nameof(ConstructorInstantiationInfoFactory)}\" or you messed with the .NET type system badly.");
        }

        private static bool MatchConstructorParameters(ParameterInfo[] constructedGenericTypeParameters,
                                                       ParameterInfo[] genericTypeDefinitionParameters,
                                                       TypeInfo constructedGenericTypeInfo,
                                                       TypeInfo genericTypeDefinitionInfo)
        {
            for (var i = 0; i < constructedGenericTypeParameters.Length; i++)
            {
                var firstParameter = constructedGenericTypeParameters[i];
                var secondParameter = genericTypeDefinitionParameters[i];
                var resolvedParameterType = secondParameter.ParameterType.IsGenericParameter
                                                ? ResolveGenericParameter(secondParameter.ParameterType, constructedGenericTypeInfo, genericTypeDefinitionInfo)
                                                : secondParameter.ParameterType;

                if (resolvedParameterType != firstParameter.ParameterType)
                    return false;
            }
            return true;
        }

        private static Type ResolveGenericParameter(Type parameterType, TypeInfo constructedTypeInfo, TypeInfo genericTypeDefinitionInfo)
        {
            var targetIndex = -1;
            for (var i = 0; i < genericTypeDefinitionInfo.GenericTypeParameters.Length; i++)
            {
                if (genericTypeDefinitionInfo.GenericTypeParameters[i] == parameterType)
                    targetIndex = i;
            }

            return constructedTypeInfo.GenericTypeArguments[targetIndex];
        }
    }
}