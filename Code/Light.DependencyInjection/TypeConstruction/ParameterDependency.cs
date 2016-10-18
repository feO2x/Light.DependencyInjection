using System;
using System.Reflection;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ParameterDependency : ISetTargetRegistrationName
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

        public string TargetRegistrationName
        {
            get { return _childValueRegistrationName; }
            set { _childValueRegistrationName = value; }
        }

        public object ResolveDependency(CreationContext context)
        {
            return context.ResolveChildValue(new TypeKey(ParameterType, _childValueRegistrationName));
        }
    }
}