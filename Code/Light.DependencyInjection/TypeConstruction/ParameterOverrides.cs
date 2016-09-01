using System;
using System.Diagnostics;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public struct ParameterOverrides
    {
        public readonly TypeCreationInfo TypeCreationInfo;
        public readonly object[] InstantiationParameters;

        public ParameterOverrides(TypeCreationInfo typeCreationInfo)
        {
            typeCreationInfo.MustNotBeNull(nameof(typeCreationInfo));

            TypeCreationInfo = typeCreationInfo;

            var instantiationDependencies = typeCreationInfo.InstantiationInfo.InstantiationDependencies;
            InstantiationParameters = instantiationDependencies == null || instantiationDependencies.Count == 0 ? null : new object[instantiationDependencies.Count];
        }

        public ParameterOverrides OverrideInstantiationParameter(string parameterName, object value)
        {
            EnsureInstantiationParameterNotNull();

            for (var i = 0; i < TypeCreationInfo.InstantiationInfo.InstantiationDependencies.Count; i++)
            {
                if (TypeCreationInfo.InstantiationInfo.InstantiationDependencies[i].TargetParameter.Name != parameterName)
                    continue;

                InstantiationParameters[i] = value ?? ExplicitlyPassedNull.Instance;
                return this;
            }

            throw new ResolveTypeException($"There is no parameter with name \"{parameterName}\" for the instantiation method of type \"{TypeCreationInfo.TargetType}\".", TypeCreationInfo.TargetType);
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureInstantiationParameterNotNull()
        {
            if (InstantiationParameters == null)
                throw new InvalidOperationException($"You cannot override an instantiation method parameter for type \"{TypeCreationInfo.TargetType}\" because the instantiation method is parametersless.");
        }

        public ParameterOverrides OverrideInstantiationParameter(Type type, object value)
        {
            type.MustNotBeNull(nameof(type));
            EnsureInstantiationParameterNotNull();

            var targetIndex = FindSingleTargetIndexByType(type);
            InstantiationParameters[targetIndex] = value ?? ExplicitlyPassedNull.Instance;
            return this;
        }

        public ParameterOverrides OverrideInstantiationParameter<T>(object value)
        {
            OverrideInstantiationParameter(typeof(T), value);
            return this;
        }

        private int FindSingleTargetIndexByType(Type parameterType)
        {
            var targetIndex = -1;
            for (var i = 0; i < TypeCreationInfo.InstantiationInfo.InstantiationDependencies.Count; ++i)
            {
                if (TypeCreationInfo.InstantiationInfo.InstantiationDependencies[i].ParameterType != parameterType)
                    continue;

                if (targetIndex != -1)
                    throw new ResolveTypeException($"You cannot override the parameter with type \"{parameterType}\" because there are several parameters with this type in the instantiation method for type \"{TypeCreationInfo.TargetType}\".", TypeCreationInfo.TargetType);

                targetIndex = i;
            }

            if (targetIndex == -1)
                throw new ResolveTypeException($"The instantiation method of type \"{TypeCreationInfo.TargetType}\" has no parameter with type \"{parameterType}\".", TypeCreationInfo.TargetType);

            return targetIndex;
        }
    }
}