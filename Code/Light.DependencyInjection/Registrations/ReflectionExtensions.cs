using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public static class ReflectionExtensions
    {
        public static ConstructorInfo FindDefaultConstructor(this IEnumerable<ConstructorInfo> constructors)
        {
            var constructorList = constructors.AsReadOnlyList();

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructor = constructorList[i];
                if (constructor.GetParameters().Length == 0)
                    return constructor;
            }

            return null;
        }

        public static ConstructorInfo FindConstructorWithArgumentTypes(this IEnumerable<ConstructorInfo> constructors, params Type[] parameterTypes)
        {
            parameterTypes.MustNotBeNull(nameof(parameterTypes));

            var constructorList = constructors.AsReadOnlyList();

            for (var i = 0; i < constructorList.Count; i++)
            {
                var constructorInfo = constructorList[i];
                var parameterInfos = constructorInfo.GetParameters();

                if (parameterInfos.CheckParameterEquality(parameterTypes))
                    return constructorInfo;
            }

            return null;
        }

        public static bool CheckParameterEquality(this ParameterInfo[] parameterInfos, Type[] parameterTypes, IEqualityComparer<Type> typeComparer = null)
        {
            if (parameterInfos.MustNotBeNull(nameof(parameterInfos)).Length != parameterTypes.MustNotBeNull(nameof(parameterTypes)).Length)
                return false;

            typeComparer = typeComparer ?? EqualityComparer<Type>.Default;

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                if (typeComparer.Equals(parameterInfos[i].ParameterType, parameterTypes[i]) == false)
                    return false;
            }
            return true;
        }

        public static IReadOnlyList<DependencyFactory> ExtractDependency(this PropertyInfo propertyInfo)
        {
            return propertyInfo.MustNotBeNull(nameof(propertyInfo)).SetMethod.ExtractDependencies();
        }

        public static IReadOnlyList<DependencyFactory> ExtractDependency(this FieldInfo fieldInfo)
        {
            fieldInfo.MustNotBeNull(nameof(fieldInfo));
            return new[] { new DependencyFactory(fieldInfo.Name, fieldInfo.FieldType) };
        }

        public static IReadOnlyList<DependencyFactory> ExtractDependencies(this MethodBase instantiationMethod)
        {
            instantiationMethod.MustNotBeNull(nameof(instantiationMethod));

            return instantiationMethod.GetParameters()
                                      .Select(parameterInfo => new DependencyFactory(parameterInfo.Name, parameterInfo.ParameterType))
                                      .ToList();
        }

        public static IReadOnlyList<Dependency> CreateDependencies(this IReadOnlyList<DependencyFactory> dependencyFactories)
        {
            if (dependencyFactories.MustNotBeNull(nameof(dependencyFactories)).Count == 0)
                return null;

            var array = new Dependency[dependencyFactories.Count];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = dependencyFactories[i].Create();
            }

            return array;
        }

        public static IReadOnlyList<InstanceManipulation> CreateInstanceManipulations(this IReadOnlyList<InstanceManipulationFactory> instanceManipulationFactories, string registrationName = "")
        {
            if (instanceManipulationFactories.MustNotBeNull(nameof(instanceManipulationFactories)).Count == 0)
                return null;

            var array = new InstanceManipulation[instanceManipulationFactories.Count];

            for (var i = 0; i < instanceManipulationFactories.Count; i++)
            {
                array[i] = instanceManipulationFactories[i].Create(registrationName);
            }

            return array;
        }
    }
}