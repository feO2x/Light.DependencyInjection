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
            return TypeInstantiationInfo.FromTypeInstantiatedByDiContainer(type, targetConstructor, targetConstructor.CompileObjectCreationFunction());
        }
    }
}