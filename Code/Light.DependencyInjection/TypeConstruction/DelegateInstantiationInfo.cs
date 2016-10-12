using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DelegateInstantiationInfo : InstantiationInfo
    {
        public DelegateInstantiationInfo(Delegate @delegate)
            : base(@delegate.GetMethodInfo().ReturnType,
                   @delegate.CompileStandardizedInstantiationFunction(),
                   @delegate.GetMethodInfo().CreateDefaultInstantiationDependencies()) { }
        protected override InstantiationInfo CloneForClosedConstructedGenericTypeInternal(Type closedConstructedGenericType, TypeInfo closedConstructedGenericTypeInfo)
        {
            throw new NotSupportedException("A delegate cannot be instantiated with a generic type definition.");
        }
    }
}