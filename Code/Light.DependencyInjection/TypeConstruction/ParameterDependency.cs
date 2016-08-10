using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ParameterDependency : ISetChildValueRegistrationName
    {
        public readonly Type ParameterType;
        public readonly ParameterInfo TargetParameter;
        private string _childValueRegistrationName;

        public ParameterDependency(ParameterInfo targetParameter, string childValueRegistrationName = null)
        {
            targetParameter.MustNotBeNull(nameof(targetParameter));

            TargetParameter = targetParameter;
            _childValueRegistrationName = childValueRegistrationName;
            ParameterType = targetParameter.ParameterType;
        }

        public string ChildValueRegistrationName
        {
            get { return _childValueRegistrationName; }
            set { _childValueRegistrationName = value; }
        }

        public object ResolveDependency(DiContainer container)
        {
            return container.Resolve(ParameterType, _childValueRegistrationName);
        }
    }
}