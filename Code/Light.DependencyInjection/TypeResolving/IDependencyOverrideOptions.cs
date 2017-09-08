using Light.DependencyInjection.Registrations;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IDependencyOverrideOptions
    {
        Registration TargetRegistration { get; }

        IDependencyOverrideOptions Override<TDependency>(TDependency value);
    }
}