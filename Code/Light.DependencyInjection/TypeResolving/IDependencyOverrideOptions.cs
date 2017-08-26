namespace Light.DependencyInjection.TypeResolving
{
    public interface IDependencyOverrideOptions
    {
        IDependencyOverrideOptions Override<TDependency>(TDependency value);
    }
}