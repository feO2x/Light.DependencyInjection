using System;
using System.Collections.Generic;
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
    }
}