using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public struct TypeInstantiationInfo
    {
        public readonly Type TargetType;
        public readonly MethodBase TargetCreationMethodInfo;
        private readonly Func<object[], object> _createObject;

        public TypeInstantiationInfo(Type targetType, MethodBase creationMethodInfo, Func<object[], object> createObject)
        {
            targetType.MustNotBeNull(nameof(targetType));
            creationMethodInfo.MustNotBeNull(nameof(creationMethodInfo));

            TargetType = targetType;
            TargetCreationMethodInfo = creationMethodInfo;
            _createObject = createObject;
        }

        [Pure]
        public object InstatiateObject(DiContainer container)
        {
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
    }
}