using System;
using System.Reflection;
using Light.DependencyInjection.FrameworkExtensions;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents an <see cref="InstantiationInfo" /> that calls the method that is associated with a delegate internally.
    /// </summary>
    public sealed class DelegateInstantiationInfo : InstantiationInfo
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="DelegateInstantiationInfo" />.
        /// </summary>
        /// <param name="targetType">The target type that will be instantiated.</param>
        /// <param name="delegate">The delegate whose referenced method will be called to instantiate the target type.</param>
        public DelegateInstantiationInfo(Type targetType, Delegate @delegate)
            : base(targetType,
                   @delegate.CompileStandardizedInstantiationFunction(targetType),
                   @delegate.GetMethodInfo().CreateDefaultInstantiationDependencies()) { }

        /// <inheritdoc />
        protected override InstantiationInfo BindToClosedGenericTypeInternal(Type closedGenericType, TypeInfo closedGenericTypeInfo)
        {
            throw new NotSupportedException("A delegate cannot be instantiated with a generic type definition.");
        }
    }
}