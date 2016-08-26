using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class StaticMethodInstantiationInfo : InstantiationInfo
    {
        public readonly MethodInfo StaticMethodInfo;

        public StaticMethodInstantiationInfo(MethodInfo staticMethodInfo) :
            base(staticMethodInfo.ReturnType,
                 staticMethodInfo.CompileStandardizedInstantiationFunction(),
                 staticMethodInfo.CreateDefaultInstantiationDependencies())
        {
            StaticMethodInfo = staticMethodInfo;
        }
        protected override InstantiationInfo CloneForBoundGenericTypeInternal(Type boundGenericType, TypeInfo boundGenericTypeInfo)
        {
            return new StaticMethodInstantiationInfo(StaticMethodInfo.MakeGenericMethod(boundGenericType.GenericTypeArguments));
        }
    }
}