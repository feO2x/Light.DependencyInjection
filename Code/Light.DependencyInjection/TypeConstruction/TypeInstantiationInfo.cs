using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeInstantiationInfo
    {
        private readonly List<ParameterDependency> _parameterInfos;
        public readonly MethodBase InstantiationMethodInfo;
        public readonly TypeCreationKind Kind;
        public readonly Func<object[], object> StandardizedInstantiationFunction;
        public readonly Type TargetType;

        private TypeInstantiationInfo(Type targetType, MethodBase instantiationMethodInfo, Func<object[], object> standardizedInstantiationFunction, List<ParameterDependency> parameterInfos, TypeCreationKind kind)
        {
            targetType.MustNotBeNull(nameof(targetType));
            instantiationMethodInfo.MustNotBeNull(nameof(instantiationMethodInfo));

            TargetType = targetType;
            InstantiationMethodInfo = instantiationMethodInfo;
            StandardizedInstantiationFunction = standardizedInstantiationFunction;
            _parameterInfos = parameterInfos;
            Kind = kind;
        }

        public IReadOnlyList<ParameterDependency> ParameterInfos => _parameterInfos;

        public object Instantiate(DiContainer container)
        {
            if (_parameterInfos == null || _parameterInfos.Count == 0)
                return StandardizedInstantiationFunction(null);

            var parameters = new object[_parameterInfos.Count];

            for (var i = 0; i < _parameterInfos.Count; i++)
            {
                parameters[i] = _parameterInfos[i].ResolveDependency(container);
            }

            return StandardizedInstantiationFunction(parameters);
        }

        public static TypeInstantiationInfo FromResolvableType(Type targetType, MethodBase instantiationMethodInfo, Func<object[], object> standardizedInstantiationFunction, List<ParameterDependency> parameterInfos)
        {
            CheckIfTypeIsInstantiatable(targetType);
            instantiationMethodInfo.MustNotBeNull(nameof(instantiationMethodInfo));
            standardizedInstantiationFunction.MustNotBeNull(nameof(standardizedInstantiationFunction));
            CheckParameterInfos(instantiationMethodInfo, parameterInfos);

            return new TypeInstantiationInfo(targetType, instantiationMethodInfo, standardizedInstantiationFunction, parameterInfos, TypeCreationKind.InstantiatedByDiContainer);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckIfTypeIsInstantiatable(Type targetType)
        {
            var typeInfo = targetType.GetTypeInfo();
            if (typeInfo.IsAbstract || typeInfo.IsGenericTypeDefinition || typeInfo.IsInterface)
                throw new TypeRegistrationException($"The specified type \"{targetType}\" is not an instantiatable type.", targetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckParameterInfos(MethodBase instantiationMethodInfo, List<ParameterDependency> parameterDependencies)
        {
            var parameterInfos = instantiationMethodInfo.GetParameters();
            if (parameterInfos.Length == 0 && parameterDependencies == null)
                return;
            if (parameterDependencies.Select(d => d.TargetParameter).SequenceEqual(parameterInfos) == false)
                throw new TypeRegistrationException($"The specified {nameof(parameterDependencies)} do not describe the same parameters as the instantiation method \"{instantiationMethodInfo}\".", instantiationMethodInfo.DeclaringType);
        }

        public static TypeInstantiationInfo FromUnboundGenericType(Type targetType, MethodBase instantiationMethodInfo, List<ParameterDependency> parameterDependencies)
        {
            return new TypeInstantiationInfo(targetType, instantiationMethodInfo, null, parameterDependencies, TypeCreationKind.UnboundGenericTypeTemplate);
        }
    }
}