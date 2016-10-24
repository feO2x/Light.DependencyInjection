using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public abstract class InstantiationInfo
    {
        private readonly InstantiationDependency[] _instantiationDependencies;
        public readonly Func<object[], object> StandardizedInstantiationFunction;
        public readonly Type TargetType;
        public readonly TypeInfo TargetTypeInfo;

        protected InstantiationInfo(Type targetType, Func<object[], object> standardizedInstantiationFunction, InstantiationDependency[] instantiationDependencies)
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

        public IReadOnlyList<InstantiationDependency> InstantiationDependencies => _instantiationDependencies;

        public virtual object Instantiate(CreationContext context)
        {
            if (InstantiationDependencies == null || _instantiationDependencies.Length == 0)
                return StandardizedInstantiationFunction(null);

            // Check if there is something to override
            if (context.ParameterOverrides == null)
            {
                // If not, use the context (i.e. the DI container) to resolve all dependencies
                var parameters = new object[_instantiationDependencies.Length];

                for (var i = 0; i < parameters.Length; ++i)
                {
                    parameters[i] = _instantiationDependencies[i].ResolveDependency(context);
                }
                return StandardizedInstantiationFunction(parameters);
            }

            // Else use the array of the ParameterOverrides to instantiate the object
            var parameterOverrides = context.ParameterOverrides.Value;
            for (var i = 0; i < parameterOverrides.InstantiationParameters.Length; ++i)
            {
                var instantiationParameter = parameterOverrides.InstantiationParameters[i];
                if (instantiationParameter == null)
                    parameterOverrides.InstantiationParameters[i] = _instantiationDependencies[i].ResolveDependency(context);
                else if (instantiationParameter is ExplicitlyPassedNull)
                    parameterOverrides.InstantiationParameters[i] = null;
            }

            return StandardizedInstantiationFunction(parameterOverrides.InstantiationParameters);
        }

        public InstantiationInfo BindToClosedGenericType(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            closedGenericType.MustBeClosedVariantOf(TargetType);

            return CloneForClosedConstructedGenericTypeInternal(closedGenericType, closedGenericTypeInfo);
        }

        protected abstract InstantiationInfo CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo);
    }
}