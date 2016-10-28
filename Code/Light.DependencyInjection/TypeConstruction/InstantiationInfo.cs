using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents a standardized set of information allowing the instantiation of a target type.
    /// </summary>
    public abstract class InstantiationInfo
    {
        private readonly InstantiationDependency[] _instantiationDependencies;

        /// <summary>
        ///     Gets the standardized instantiation function.
        /// </summary>
        public readonly Func<object[], object> StandardizedInstantiationFunction;

        /// <summary>
        ///     Gets the type that can be instantiated with this info.
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        ///     Gets the type info of the target type.
        /// </summary>
        public readonly TypeInfo TargetTypeInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="InstantiationInfo" />.
        /// </summary>
        /// <param name="targetType">The type can be instantiated with this info.</param>
        /// <param name="standardizedInstantiationFunction">The Standardized Instantiation Function which actually creates instances of the target type. Can be null if the target type describes a generic type definition.</param>
        /// <param name="instantiationDependencies">Information about the parameters that have to be injected into the standardized instantiation function (SIF). Can be null if the SIF does not take any parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetType" /> is null, or <paramref name="standardizedInstantiationFunction" /> is null and the <paramref name="targetType" /> is no generic type definition.</exception>
        /// <exception cref="TypeRegistrationException">Thrown when the target type is an interface, abstract class, generic parameter, or open generic type.</exception>
        protected InstantiationInfo(Type targetType, Func<object[], object> standardizedInstantiationFunction, InstantiationDependency[] instantiationDependencies)
        {
            targetType.MustBeRegistrationCompliant();
            TargetTypeInfo = targetType.GetTypeInfo();

            CheckStandardizedInstantiationFunction(standardizedInstantiationFunction);

            TargetType = targetType;
            StandardizedInstantiationFunction = standardizedInstantiationFunction;
            _instantiationDependencies = instantiationDependencies;
        }

        /// <summary>
        ///     Gets information about the parameters that have to be injected into the Standardized Instantiation Function.
        /// </summary>
        public IReadOnlyList<InstantiationDependency> InstantiationDependencies => _instantiationDependencies;

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckStandardizedInstantiationFunction(Func<object[], object> standardizedInstantiationFunction)
        {
            if (TargetTypeInfo.IsGenericTypeDefinition)
                return;

            standardizedInstantiationFunction.MustNotBeNull(nameof(standardizedInstantiationFunction));
        }

        /// <summary>
        ///     Creates a new instance of the target type.
        /// </summary>
        /// <param name="context">The context used to resolve child values recursively.</param>
        /// <returns>The new instance of the target type.</returns>
        public virtual object Instantiate(ResolveContext context)
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

        /// <summary>
        ///     Creates a new <see cref="InstantiationInfo" /> for the <paramref name="closedGenericType" /> with the
        ///     settings copied from this instance. You may only call this method if the target type is a generic type definition.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="closedGenericType" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the target type is not a generic type definition.</exception>
        /// <exception cref="ResolveTypeException">Thrown when <paramref name="closedGenericType" />is not a closed generic type variant of the target type.</exception>
        public InstantiationInfo BindToClosedGenericType(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            closedGenericType.MustBeClosedVariantOf(TargetType);

            return BindToClosedGenericTypeInternal(closedGenericType, closedGenericTypeInfo);
        }

        /// <summary>
        ///     Creates a new instance of the derived class.
        /// </summary>
        protected abstract InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo);
    }
}