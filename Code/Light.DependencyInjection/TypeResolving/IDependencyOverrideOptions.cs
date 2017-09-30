using System;
using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IDependencyOverrideOptions
    {
        Registration TargetRegistration { get; }

        IDependencyOverrideOptions OverrideDependency<TDependency>(TDependency value);
        IDependencyOverrideOptions OverrideDependency<TDependency>(string dependencyName, TDependency value, StringComparison nameComparisonType = StringComparison.CurrentCulture);
        IDependencyOverrideOptions OverrideDependency(Type dependencyType, object dependencyValue);
        IDependencyOverrideOptions OverrideDependency(string dependencyName, object dependencyValue, StringComparison nameComparisonType = StringComparison.CurrentCulture);
        IDependencyOverrideOptions OverrideRegistration<T>(T value, string registrationName = "");
        IDependencyOverrideOptions OverrideRegistration(Type type, object value, string registrationName = "");
    }
}