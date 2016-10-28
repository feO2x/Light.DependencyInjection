using System;
using System.Reflection;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class InstantiationDependency : ISetTargetRegistrationName
    {
        public readonly Type ParameterType;
        public readonly ParameterInfo TargetParameter;
        private IDependencyResolver _dependencyResolver = DefaultDependencyResolver.Instance;
        public string TargetRegistrationName;

        public InstantiationDependency(ParameterInfo targetParameter, string targetRegistrationName = null)
        {
            targetParameter.MustNotBeNull(nameof(targetParameter));

            TargetParameter = targetParameter;
            TargetRegistrationName = targetRegistrationName;
            ParameterType = targetParameter.ParameterType;
        }

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

        public object ResolveDependency(ResolveContext context)
        {
            return _dependencyResolver.Resolve(ParameterType, TargetRegistrationName, context);
        }
    }
}