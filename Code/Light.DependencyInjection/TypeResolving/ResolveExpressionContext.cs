using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeResolving
{
    public struct ResolveExpressionContext
    {
        public readonly TypeKey RequestedTypeKey;
        public readonly Registration Registration;
        public readonly DependencyInjectionContainer Container;
        public readonly Type ResolvedGenericRegistrationType;
        public readonly TypeInfo RegistrationTypeInfo;
        public readonly TypeInfo ResolvedGenericRegistrationTypeInfo;
        public readonly DependencyOverrides DependencyOverrides;
        public readonly ParameterExpression ResolveContextParameterExpression;

        public ResolveExpressionContext(TypeKey requestedTypeKey,
                                        Registration registration,
                                        DependencyOverrides dependencyOverrides,
                                        DependencyInjectionContainer container,
                                        ParameterExpression resolveContextParameterExpression)
        {
            RequestedTypeKey = requestedTypeKey.MustNotBeEmpty(nameof(requestedTypeKey));
            Registration = registration.MustNotBeNull(nameof(registration));
            DependencyOverrides = dependencyOverrides;
            Container = container.MustNotBeNull(nameof(container));
            ResolveContextParameterExpression = resolveContextParameterExpression.MustNotBeNull(nameof(resolveContextParameterExpression));
            RegistrationTypeInfo = registration.TypeKey.Type.GetTypeInfo();
            ResolvedGenericRegistrationType = null;
            ResolvedGenericRegistrationTypeInfo = null;

            // ReSharper disable once PossibleNullReferenceException
            if (RegistrationTypeInfo.IsGenericTypeDefinition == false)
                return;

            try
            {
                ResolvedGenericRegistrationType = registration.TargetType.MakeGenericType(RequestedType.GenericTypeArguments);
                ResolvedGenericRegistrationTypeInfo = ResolvedGenericRegistrationType.GetTypeInfo();
            }
            catch (Exception exception)
            {
                throw new ResolveException($"Could not resolve a concrete instance of the generic type definition \"{RegistrationType}\" using the type \"{RequestedType}\".", exception);
            }
        }

        public Type RequestedType => RequestedTypeKey.Type;
        public Type RegistrationType => Registration.TargetType;
        public Type InstanceType => ResolvedGenericRegistrationType ?? RegistrationType;
        public bool IsResolvingGenericTypeDefinition => RegistrationType.IsGenericTypeDefinition();

        public Type ResolveGenericTypeParameterIfNecessary(Type type)
        {
            return type.IsGenericTypeParameter() == false ? type : ResolveGenericTypeParameter(type);
        }

        private Type ResolveGenericTypeParameter(Type type)
        {
            for (var i = 0; i < RegistrationTypeInfo.GenericTypeParameters.Length; i++)
            {
                if (RegistrationTypeInfo.GenericTypeParameters[i] == type)
                    return ResolvedGenericRegistrationTypeInfo.GenericTypeArguments[i];
            }

            throw new ArgumentException($"The type \"{type}\" does not correspond to a generic parameter of type \"{RegistrationType}\".", nameof(type));
        }

        public T FindResolvedGenericMethod<T>(T genericMethods, IReadOnlyList<T> constructedMethods) where T : MethodBase
        {
            var parametersOfGenericMethod = genericMethods.GetParameters();
            for (var i = 0; i < constructedMethods.Count; i++)
            {
                var targetMethod = constructedMethods[i];
                var parameters = targetMethod.GetParameters();
                if (MatchParameters(parameters, parametersOfGenericMethod))
                    return targetMethod;
            }
            return null;
        }

        public bool MatchParameters(ParameterInfo[] constructedGenericTypeParameters,
                                    ParameterInfo[] genericTypeDefinitionParameters)
        {
            if (constructedGenericTypeParameters.Length != genericTypeDefinitionParameters.Length)
                return false;

            for (var i = 0; i < constructedGenericTypeParameters.Length; i++)
            {
                var firstParameter = constructedGenericTypeParameters[i];
                var secondParameter = genericTypeDefinitionParameters[i];
                var resolvedParameterType = ResolveGenericTypeParameterIfNecessary(secondParameter.ParameterType);
                resolvedParameterType = ResolveOpenConstructedGenericTypeIfNecessary(resolvedParameterType);

                if (resolvedParameterType != firstParameter.ParameterType)
                    return false;
            }
            return true;
        }

        public Type ResolveOpenConstructedGenericTypeIfNecessary(Type dependencyType)
        {
            var openConstructedGenericTypeInfo = dependencyType.GetTypeInfo();

            if (openConstructedGenericTypeInfo.IsOpenConstructedGenericType() == false)
                return dependencyType;

            var resolvedGenericParameters = new Type[openConstructedGenericTypeInfo.GenericTypeArguments.Length];
            for (var i = 0; i < resolvedGenericParameters.Length; i++)
            {
                var typeArgument = openConstructedGenericTypeInfo.GenericTypeArguments[i];
                if (typeArgument.IsGenericParameter == false)
                    resolvedGenericParameters[i] = typeArgument;
                else
                    resolvedGenericParameters[i] = ResolveGenericTypeParameter(typeArgument);
            }

            return dependencyType.GetGenericTypeDefinition().MakeGenericType(resolvedGenericParameters);
        }

        public Expression CreateInstantiationAndTrackDisposableExpression(Expression createInstanceExpression)
        {
            if (Registration.IsTrackingDisposables == false)
                return createInstanceExpression;

            var createAndTrackExpressions = new Expression[3];
            var variableExpression = Expression.Variable(InstanceType);
            createAndTrackExpressions[0] = Expression.Assign(variableExpression, createInstanceExpression);
            createAndTrackExpressions[1] = 
                Expression.Call(
                    Expression.Call(
                        Expression.Call(ResolveContextParameterExpression)))
        }
    }
}