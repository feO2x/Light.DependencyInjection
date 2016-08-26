using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class TypeAnalyzer
    {
        private IConstructorSelector _constructorSelector = new ConstructorWithMostParametersSelector();

        private IReadOnlyList<Type> _ignoredAbstractionTypes = new[]
                                                               {
                                                                   typeof(IDisposable)
                                                               };


        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _constructorSelector = value;
            }
        }

        public IReadOnlyList<Type> IgnoredAbstractionTypes
        {
            get { return _ignoredAbstractionTypes; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _ignoredAbstractionTypes = value;
            }
        }

        public TypeCreationInfo CreateInfoFor(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var typeInfo = type.GetTypeInfo();
            var targetConstructor = _constructorSelector.SelectTargetConstructor(typeInfo);
            return new TypeCreationInfo(new ConstructorInstantiationInfo(targetConstructor));
        }
    }
}