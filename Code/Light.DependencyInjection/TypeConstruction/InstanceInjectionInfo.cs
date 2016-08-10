using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class InstanceInjectionInfo
    {
        private readonly List<InstanceInjection> _instanceInjections;
        public IReadOnlyList<InstanceInjection> InstanceInjections => _instanceInjections;

        public InstanceInjectionInfo(List<InstanceInjection> instanceInjections)
        {
            instanceInjections.MustNotBeNullOrEmpty(nameof(instanceInjections));

            _instanceInjections = instanceInjections;
        }

        public void PerformInjections(object instance, DiContainer container)
        {
            foreach (var instanceInjection in _instanceInjections)
            {
                instanceInjection.InjectValue(instance, container.Resolve(instanceInjection.MemberType, instanceInjection.ChildValueRegistrationName));
            }
        }
    }
}