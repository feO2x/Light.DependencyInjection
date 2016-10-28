using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="InstantiationInfo" /> that calls a constructor.
    /// </summary>
    public sealed class ConstructorInstantiationInfo : InstantiationInfo
    {
        /// <summary>
        ///     Gets the info for the constructor the is called by this instance.
        /// </summary>
        public readonly ConstructorInfo ConstructorInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="ConstructorInstantiationInfo" />.
        /// </summary>
        /// <param name="constructorInfo">The constructor that will be called to instantiate the target type.</param>
        public ConstructorInstantiationInfo(ConstructorInfo constructorInfo)
            : base(constructorInfo.DeclaringType,
                   constructorInfo.CompileStandardizedInstantiationFunction(),
                   constructorInfo.CreateDefaultInstantiationDependencies())
        {
            ConstructorInfo = constructorInfo;
        }

        /// <inheritdoc />
        protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            if (InstantiationDependencies == null || InstantiationDependencies.Count == 0)
                return new ConstructorInstantiationInfo(closedGenericTypeInfo.GetDefaultConstructor());

            var boundConstructors = closedGenericTypeInfo.DeclaredConstructors.AsList();
            if (boundConstructors.Count == 1)
                return new ConstructorInstantiationInfo(boundConstructors[0]);

            var unboundConstructorParameters = ConstructorInfo.GetParameters();
            for (var i = 0; i < boundConstructors.Count; i++)
            {
                var boundConstructor = boundConstructors[i];
                var boundConstructorParameters = boundConstructor.GetParameters();
                if (unboundConstructorParameters.Length != boundConstructorParameters.Length)
                    continue;

                if (MatchConstructorParameters(boundConstructorParameters, unboundConstructorParameters, closedGenericTypeInfo))
                    return new ConstructorInstantiationInfo(boundConstructor);
            }
            throw new ResolveTypeException($"The constructor for the closed constructed generic type \"{closedGenericType}\" that matches the generic type definition's constructor \"{ConstructorInfo}\" could not be found. This exception should actually never happen, unless there is a bug in class \"{nameof(ConstructorInstantiationInfo)}\" or you messed with the .NET type system badly.", closedGenericType);
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