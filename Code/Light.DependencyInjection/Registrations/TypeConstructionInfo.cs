using System.Collections.Generic;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public sealed class TypeConstructionInfo : IDependencyInfo
    {
        public readonly IReadOnlyList<Dependency> AllDependencies;
        public readonly IReadOnlyList<InstanceManipulation> InstanceManipulations;
        public readonly InstantiationInfo InstantiationInfo;
        public readonly TypeKey TypeKey;

        public TypeConstructionInfo(TypeKey typeKey, InstantiationInfo instantiationInfo, IReadOnlyList<InstanceManipulation> instanceManipulations = null)
        {
            InstantiationInfo = instantiationInfo.MustNotBeNull(nameof(instantiationInfo));
            TypeKey = typeKey.MustBe(instantiationInfo.TypeKey, parameterName: nameof(typeKey));
            instanceManipulations?.ForEach(instanceManipulation => instanceManipulation.TypeKey.MustBe(typeKey));
            InstanceManipulations = instanceManipulations;
            AllDependencies = InstantiationInfo.InstantiationDependencies;
            if (instanceManipulations.IsNullOrEmpty()) return;

            var dependencies = AllDependencies.IsNullOrEmpty() ? new List<Dependency>() : new List<Dependency>(AllDependencies);
            // ReSharper disable once PossibleNullReferenceException
            for (var i = 0; i < instanceManipulations.Count; i++)
            {
                var manipulation = instanceManipulations[i];
                if (manipulation.Dependencies.IsNullOrEmpty())
                    continue;

                dependencies.AddRange(manipulation.Dependencies);
            }
            AllDependencies = dependencies;
        }

        IReadOnlyList<Dependency> IDependencyInfo.Dependencies => AllDependencies;
    }
}