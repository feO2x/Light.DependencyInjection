using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ConstructorInstantiationInfo : InstantiationInfo
    {
        public readonly ConstructorInfo ConstructorInfo;

        public ConstructorInstantiationInfo(ConstructorInfo constructorInfo)
            : base(constructorInfo.DeclaringType,
                   constructorInfo.CompileStandardizedInstantiationFunction(),
                   constructorInfo.CreateDefaultInstantiationDependencies())
        {
            ConstructorInfo = constructorInfo;
        }

        protected override InstantiationInfo CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            if (InstantiationDependencies == null || InstantiationDependencies.Count == 0)
                return new ConstructorInstantiationInfo(closedConstructedGenericTypeInfo.GetDefaultConstructor());

            var boundConstructors = closedConstructedGenericTypeInfo.DeclaredConstructors.AsList();
            if (boundConstructors.Count == 1)
                return new ConstructorInstantiationInfo(boundConstructors[0]);

            var unboundConstructorParameters = ConstructorInfo.GetParameters();
            for (var i = 0; i < boundConstructors.Count; i++)
            {
                var boundConstructor = boundConstructors[i];
                var boundConstructorParameters = boundConstructor.GetParameters();
                if (unboundConstructorParameters.Length != boundConstructorParameters.Length)
                    continue;

                if (MatchConstructorParameters(boundConstructorParameters, unboundConstructorParameters, closedConstructedGenericTypeInfo))
                    return new ConstructorInstantiationInfo(boundConstructor);
            }
            throw new ResolveTypeException($"The constructor for the bound generic type \"{closedConstructedGenericType}\" that matches the unbound version \"{ConstructorInfo}\" could not be found.", closedConstructedGenericType);
        }

        private bool MatchConstructorParameters(ParameterInfo[] boundConstructorParameters,
                                                ParameterInfo[] unboundConstructorParameters,
                                                TypeInfo boundGenericTypeInfo)
        {
            for (var i = 0; i < boundConstructorParameters.Length; i++)
            {
                var boundParameter = boundConstructorParameters[i];
                var unboundParameter = unboundConstructorParameters[i];
                var resolvedParameterType = unboundParameter.ParameterType.IsGenericParameter ? ResolveGenericParameter(unboundParameter.ParameterType, boundGenericTypeInfo)
                                                : unboundParameter.ParameterType;

                if (resolvedParameterType != boundParameter.ParameterType)
                    return false;
            }
            return true;
        }

        private Type ResolveGenericParameter(Type unboundParameterType, TypeInfo boundGenericTypeInfo)
        {
            var targetIndex = -1;
            for (var i = 0; i < TargetTypeInfo.GenericTypeParameters.Length; i++)
            {
                if (TargetTypeInfo.GenericTypeParameters[i] == unboundParameterType)
                    targetIndex = i;
            }

            return boundGenericTypeInfo.GenericTypeArguments[targetIndex];
        }
    }
}