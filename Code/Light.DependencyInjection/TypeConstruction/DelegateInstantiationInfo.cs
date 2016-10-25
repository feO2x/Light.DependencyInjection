using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DelegateInstantiationInfo : InstantiationInfo
    {
        public DelegateInstantiationInfo(Type targetType, Delegate @delegate)
            : base(targetType,
                   @delegate.CompileStandardizedInstantiationFunction(),
                   @delegate.GetMethodInfo().CreateDefaultInstantiationDependencies()) { }

        protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            throw new NotSupportedException("A delegate cannot be instantiated with a generic type definition.");
        }
    }
}