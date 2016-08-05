using System;
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
        public readonly MethodBase TargetCreationMethodInfo;
        private readonly Func<object[], object> _createObject;

        private TypeInstantiationInfo(Type targetType, MethodBase creationMethodInfo, Func<object[], object> createObject)
        {
            targetType.MustNotBeNull(nameof(targetType));
            creationMethodInfo.MustNotBeNull(nameof(creationMethodInfo));

            TargetType = targetType;
            TargetCreationMethodInfo = creationMethodInfo;
            _createObject = createObject;
            Kind = TypeInstantiationKind.CreatedByDiContainer;
        }

        private TypeInstantiationInfo(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            TargetType = targetType;
            TargetCreationMethodInfo = null;
            _createObject = null;
            Kind = TypeInstantiationKind.CreatedExternally;
        }

        public static TypeInstantiationInfo FromTypeInstantiatedByDiContainer(Type targetType, MethodBase creationMethod, Func<object[], object> createObject)
        {
            return new TypeInstantiationInfo(targetType, creationMethod, createObject);
        }

        public static TypeInstantiationInfo FromExternalInstance(object instance)
        {
            instance.MustNotBeNull(nameof(instance));

            return new TypeInstantiationInfo(instance.GetType());
        }

        [Pure]
        public object InstatiateObject(DiContainer container)
        {
            CheckKind();

            var methodParameters = TargetCreationMethodInfo.GetParameters();

            if (methodParameters.Length == 0)
                return _createObject(null);

            var parameters = new object[methodParameters.Length];
            for (var i = 0; i < methodParameters.Length; i++)
            {
                parameters[i] = container.Resolve(methodParameters[i].ParameterType);
            }

            return _createObject(parameters);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void CheckKind()
        {
            if (Kind == TypeInstantiationKind.CreatedExternally)
                throw new InvalidOperationException($"You must not call instantiate object on this instance for type {TargetType} because the requested object is not created by the DI Container, but passed in from an external source (e.g. via DiContainer.RegisterInstance).");
        }
    }
}