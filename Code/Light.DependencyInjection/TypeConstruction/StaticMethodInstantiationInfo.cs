using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class StaticMethodInstantiationInfo : InstantiationInfo
    {
        public readonly MethodInfo StaticMethodInfo;

        public StaticMethodInstantiationInfo(Type targetType, MethodInfo staticMethodInfo) :
            base(targetType,
                 staticMethodInfo.CompileStandardizedInstantiationFunction(targetType),
                 staticMethodInfo.CreateDefaultInstantiationDependencies())
        {
            StaticMethodInfo = staticMethodInfo;
        }

        protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            return new StaticMethodInstantiationInfo(closedGenericType, StaticMethodInfo.MakeGenericMethod(closedGenericType.GenericTypeArguments));
        }
    }
}