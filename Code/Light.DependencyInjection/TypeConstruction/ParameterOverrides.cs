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
            for (var i = 0; i < TypeCreationInfo.InstantiationInfo.InstantiationDependencies.Count; i++)
            {
                if (TypeCreationInfo.InstantiationInfo.InstantiationDependencies[i].TargetParameter.Name != parameterName)
                    continue;

                InstantiationParameters[i] = value ?? ExplicitlyPassedNull.Instance;
                return this;
            }

            throw new ResolveTypeException($"There is no parameter with name \"{parameterName}\" for the instantiation method of type \"{TypeCreationInfo.TargetType}\".", TypeCreationInfo.TargetType);
        }
    }
}