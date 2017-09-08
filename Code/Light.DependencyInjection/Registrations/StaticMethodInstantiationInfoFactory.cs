using System;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class StaticMethodInstantiationInfoFactory : InstantiationInfoFactory
    {
        public readonly MethodInfo StaticMethodInfo;

        public StaticMethodInstantiationInfoFactory(Type targetType, MethodInfo staticMethodInfo)
            : base(targetType, staticMethodInfo.MustNotBeNull(nameof(staticMethodInfo)).ExtractDependencies(DependencyTypes.InstantiationDependency))
        {
            StaticMethodInfo = staticMethodInfo;
        }

        public override InstantiationInfo Create(string registrationName = "")
        {
            return new StaticMethodInstantiationInfo(new TypeKey(TargetType, registrationName), StaticMethodInfo, InstantiationDependencyFactories.CreateDependencies());
        }
    }
}