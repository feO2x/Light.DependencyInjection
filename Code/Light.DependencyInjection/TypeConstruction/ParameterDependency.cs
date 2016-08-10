using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ParameterDependency : ISetResolvedRegistrationName
    {
        public readonly Type ParameterType;
        public readonly ParameterInfo TargetParameter;
        private string _resolvedRegistrationName;

        public ParameterDependency(ParameterInfo targetParameter, string resolvedRegistrationName = null)
        {
            targetParameter.MustNotBeNull(nameof(targetParameter));

            TargetParameter = targetParameter;
            _resolvedRegistrationName = resolvedRegistrationName;
            ParameterType = targetParameter.ParameterType;
        }

        public string ResolvedRegistrationName
        {
            get { return _resolvedRegistrationName; }
            set { _resolvedRegistrationName = value; }
        }

        public object ResolveDependency(DiContainer container)
        {
            return container.Resolve(ParameterType, _resolvedRegistrationName);
        }
    }
}