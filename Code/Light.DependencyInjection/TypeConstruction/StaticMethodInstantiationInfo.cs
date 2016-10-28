using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="InstantiationInfo" /> that calls a static method.
    /// </summary>
    public sealed class StaticMethodInstantiationInfo : InstantiationInfo
    {
        /// <summary>
        ///     Gets the info of the static method that is called by this instance.
        /// </summary>
        public readonly MethodInfo StaticMethodInfo;

        /// <summary>
        ///     Initializes a new instance of <see cref="StaticMethodInstantiationInfo" />.
        /// </summary>
        /// <param name="targetType">The target type that will be instantiated.</param>
        /// <param name="staticMethodInfo">The info describing the static method which instantiates the target type.</param>
        public StaticMethodInstantiationInfo(Type targetType, MethodInfo staticMethodInfo) :
            base(targetType,
                 staticMethodInfo.CompileStandardizedInstantiationFunction(targetType),
                 staticMethodInfo.CreateDefaultInstantiationDependencies())
        {
            StaticMethodInfo = staticMethodInfo;
        }

        /// <inheritdoc />
        protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            return new StaticMethodInstantiationInfo(closedGenericType, StaticMethodInfo.MakeGenericMethod(closedGenericType.GenericTypeArguments));
        }
    }
}