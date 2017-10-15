using System.Linq.Expressions;
using Light.DependencyInjection.TypeResolving;

namespace Light.DependencyInjection.Lifetimes
{
    /// <summary>
    ///     Represents a lifetime that always creates new instances.
    /// </summary>
    public sealed class TransientLifetime : Lifetime, IOptimizeLifetimeExpression
    {
        /// <summary>
        ///     Gets the singleton instance of this lifetime.
        /// </summary>
        public static readonly TransientLifetime Instance = new TransientLifetime();

        public Expression Optimize(Expression createInstanceExpression, ResolveExpressionContext context)
        {
            return context.CreateInstantiationAndTrackDisposableExpression(createInstanceExpression);
        }

        /// <summary>
        ///     Always creates a new instance using the specified resolve context.
        /// </summary>
        public override object ResolveInstance(IResolveContext resolveContext)
        {
            return resolveContext.CreateInstance();
        }

        /// <summary>
        ///     Gets the singleton instance of this lifetime.
        /// </summary>
        public override Lifetime GetLifetimeInstanceForConstructedGenericType()
        {
            return Instance;
        }
    }
}