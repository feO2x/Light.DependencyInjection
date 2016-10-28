using System;
using System.Reflection;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    /// <summary>
    ///     Represents all information for a parameter of the Standardized Instantiation Function that is able to resolve the corresponding dependency.
    /// </summary>
    public sealed class InstantiationDependency : ISetTargetRegistrationName
    {
        /// <summary>
        ///     Gets the type of the target parameter.
        /// </summary>
        public readonly Type ParameterType;

        /// <summary>
        ///     Gets the info describing the target parameter.
        /// </summary>
        public readonly ParameterInfo TargetParameter;

        private IDependencyResolver _dependencyResolver = DefaultDependencyResolver.Instance;

        /// <summary>
        ///     Gets or sets the registration name that is used to resolve the child value for the parameter.
        /// </summary>
        public string TargetRegistrationName;

        /// <summary>
        ///     Initializes a new instance of <see cref="InstantiationDependency" />.
        /// </summary>
        /// <param name="targetParameter">The info describing the target parameter.</param>
        /// <param name="targetRegistrationName">The registration name that is used to resolve the child value for the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetParameter" /> is null.</exception>
        public InstantiationDependency(ParameterInfo targetParameter, string targetRegistrationName = null)
        {
            targetParameter.MustNotBeNull(nameof(targetParameter));

            TargetParameter = targetParameter;
            TargetRegistrationName = targetRegistrationName;
            ParameterType = targetParameter.ParameterType;
        }

        /// <summary>
        ///     Gets or sets the dependency resolver being used to retrieve the child value that will be injected into the target parameter.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _dependencyResolver = value;
            }
        }

        string ISetTargetRegistrationName.TargetRegistrationName
        {
            set { TargetRegistrationName = value; }
        }

        /// <summary>
        ///     Resolves the dependency for the target parameter using the <see cref="DependencyResolver" /> and the resolve context.
        /// </summary>
        public object ResolveDependency(ResolveContext context)
        {
            return _dependencyResolver.Resolve(ParameterType, TargetRegistrationName, context);
        }
    }
}