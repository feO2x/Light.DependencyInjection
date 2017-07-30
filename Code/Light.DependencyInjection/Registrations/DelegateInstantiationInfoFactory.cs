using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class DelegateInstantiationInfoFactory : InstantiationInfoFactory
    {
        public readonly Delegate Delegate;

        public DelegateInstantiationInfoFactory(Type targetType, Delegate @delegate)
            : base(targetType.MustNotBeNull(nameof(targetType)),
                   @delegate.MustNotBeNull(nameof(@delegate)).GetMethodInfo().ExtractDependencies())
        {
            Delegate = @delegate;
        }

        public override InstantiationInfo Create(string registrationName = "")
        {
            return new DelegateInstantiationInfo(new TypeKey(TargetType, registrationName), Delegate, InstantiationDependencyFactories.CreateDependencies());
        }
    }
}