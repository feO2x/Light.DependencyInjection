using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstantiationInfo
    {
        private readonly ParameterDependency[] _instantiationDependencies;
        public readonly Func<object[], object> StandardizedInstantiationFunction;
        public readonly Type TargetType;
        public readonly TypeInfo TargetTypeInfo;

        protected InstantiationInfo(Type targetType, Func<object[], object> standardizedInstantiationFunction, ParameterDependency[] instantiationDependencies)
        {
            TargetTypeInfo = targetType.GetTypeInfo();
            CheckStandardizedInstantiationFunction(standardizedInstantiationFunction);

            TargetType = targetType;
            StandardizedInstantiationFunction = standardizedInstantiationFunction;
            _instantiationDependencies = instantiationDependencies;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckStandardizedInstantiationFunction(Func<object[], object> standardizedInstantiationFunction)
        {
            if (TargetTypeInfo.IsGenericTypeDefinition)
                return;

            standardizedInstantiationFunction.MustNotBeNull(nameof(StaticMethodInstantiationInfo));
        }

        public IReadOnlyList<ParameterDependency> InstantiationDependencies => _instantiationDependencies;

        public virtual object Instantiate(DiContainer container)
        {
            container.MustNotBeNull(nameof(container));

            if (InstantiationDependencies == null || _instantiationDependencies.Length == 0)
                return StandardizedInstantiationFunction(null);

            var parameters = new object[_instantiationDependencies.Length];

            for (var i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = _instantiationDependencies[i].ResolveDependency(container);
            }

            return StandardizedInstantiationFunction(parameters);
        }

        public virtual object Instantiate(DiContainer container, ParameterOverrides parameterOverrides)
        {
            container.MustNotBeNull(nameof(container));

            if (parameterOverrides.InstantiationParameters == null)
                return StandardizedInstantiationFunction(null);

            for (var i = 0; i < parameterOverrides.InstantiationParameters.Length; ++i)
            {
                var instantiationParameter = parameterOverrides.InstantiationParameters[i];
                if (instantiationParameter == null)
                    parameterOverrides.InstantiationParameters[i] = _instantiationDependencies[i].ResolveDependency(container);
                else if (instantiationParameter is ExplicitlyPassedNull)
                    parameterOverrides.InstantiationParameters[i] = null;
            }

            return StandardizedInstantiationFunction(parameterOverrides.InstantiationParameters);
        }

        public InstantiationInfo CloneForClosedConstructedGenericType(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            closedConstructedGenericType.MustBeClosedConstructedVariantOf(TargetType);

            return CloneForClosedConstructedGenericTypeInternal(closedConstructedGenericType, closedConstructedGenericTypeInfo);
        }

        protected abstract InstantiationInfo CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo);
    }
}