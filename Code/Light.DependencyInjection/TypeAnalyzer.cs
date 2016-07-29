using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class TypeAnalyzer
    {
        private IConstructorSelector _constructorSelector = new ConstructorWithMostParametersSelector();

        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _constructorSelector = value;
            }
        }

        public TypeInstantiationInfo CreateInfoFor(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var typeInfo = type.GetTypeInfo();
            var targetConstructor = _constructorSelector.SelectTargetConstructor(typeInfo);
            throw new NotImplementedException();
        }
    }

    public class TypeInstantiationInfo
    {
        public readonly Type _targetType;
        public readonly ConstructorInfo _targetConstructor;
        private readonly Func<object[], object> _objectCreationFunction;

        public TypeInstantiationInfo(Type targetType, ConstructorInfo targetConstructor, string infoName = null)
        {
            targetType.MustNotBeNull(nameof(targetType));
            targetConstructor.MustNotBeNull(nameof(targetConstructor));

            _targetType = targetType;
            _targetConstructor = targetConstructor;
            _objectCreationFunction = targetConstructor.CompileObjectCreationFunction();
        }
    }
}