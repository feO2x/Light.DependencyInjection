using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct TypeInstantiationInfo
    {
        public readonly TypeInstantiationKind Kind;
        public readonly Type TargetType;
        public readonly MethodBase CreationMethodInfo;
        private readonly Func<object[], object> _standardizedInstantiationFunction;
        private readonly List<InstanceInjection> _instanceInjections;

        public IReadOnlyList<InstanceInjection> InstanceInjections => _instanceInjections;

        private TypeInstantiationInfo(Type targetType, MethodBase creationMethodInfo, Func<object[], object> standardizedInstantiationFunction, List<InstanceInjection> instanceInjections)
        {
            targetType.MustNotBeNull(nameof(targetType));
            creationMethodInfo.MustNotBeNull(nameof(creationMethodInfo));

            TargetType = targetType;
            CreationMethodInfo = creationMethodInfo;
            _standardizedInstantiationFunction = standardizedInstantiationFunction;
            Kind = TypeInstantiationKind.CreatedByDiContainer;
            _instanceInjections = instanceInjections;
        }

        private TypeInstantiationInfo(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
            CreationMethodInfo = null;
            _standardizedInstantiationFunction = null;
            Kind = TypeInstantiationKind.CreatedExternally;
            _instanceInjections = null;
        }

        public static TypeInstantiationInfo FromTypeInstantiatedByDiContainer(Type targetType, MethodBase creationMethod, Func<object[], object> createObject, List<InstanceInjection> instanceInjections)
        {
            return new TypeInstantiationInfo(targetType, creationMethod, createObject, instanceInjections);
        }

        public static TypeInstantiationInfo FromExternalInstance(object instance)
        {
            instance.MustNotBeNull(nameof(instance));

            return new TypeInstantiationInfo(instance.GetType());
        }

        [Pure]
        public object InstantiateObject(DiContainer container)
        {
            CheckKind();

            var methodParameters = CreationMethodInfo.GetParameters();

            if (methodParameters.Length == 0)
                return _standardizedInstantiationFunction(null);

            var creationArguments = new object[methodParameters.Length];

            for (var i = 0; i < methodParameters.Length; i++)
            {
                creationArguments[i] = container.Resolve(methodParameters[i].ParameterType);
            }

            return _standardizedInstantiationFunction(creationArguments);
        }

        [Pure]
        public object InstantiateObjectAndPerformInstanceInjections(DiContainer container)
        {
            var newInstance = InstantiateObject(container);
            PerformInstanceInjection(newInstance, container);
            return newInstance;
        }

        public void PerformInstanceInjection(object targetObject, DiContainer container)
        {
            if (_instanceInjections == null)
                return;

            foreach (var instanceInjection in _instanceInjections)
            {
                instanceInjection.InjectValue(targetObject, container.Resolve(instanceInjection.MemberType));
            }
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckKind()
        {
            if (Kind == TypeInstantiationKind.CreatedExternally)
                throw new InvalidOperationException($"You must not call instantiate object on this instance for type {TargetType} because the requested object is not created by the DI Container, but passed in from an external source (e.g. via DiContainer.RegisterInstance).");
        }
    }
}