using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeInstantiationInfo
    {
        private readonly List<ParameterDependency> _parameterInfos;
        public readonly MethodBase InstantiationMethodInfo;
        public readonly Func<object[], object> StandardizedInstantiationFunction;
        public readonly Type TargetType;

        public TypeInstantiationInfo(Type targetType, MethodBase instantiationMethodInfo, Func<object[], object> standardizedInstantiationFunction, List<ParameterDependency> parameterInfos)
        {
            targetType.MustNotBeNull(nameof(targetType));
            instantiationMethodInfo.MustNotBeNull(nameof(instantiationMethodInfo));
            standardizedInstantiationFunction.MustNotBeNull(nameof(standardizedInstantiationFunction));

            TargetType = targetType;
            InstantiationMethodInfo = instantiationMethodInfo;
            StandardizedInstantiationFunction = standardizedInstantiationFunction;
            _parameterInfos = parameterInfos;
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
    }
}