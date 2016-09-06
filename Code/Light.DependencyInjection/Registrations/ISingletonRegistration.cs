namespace Light.DependencyInjection.Registrations
{
    public interface ISingletonRegistration
    {
        bool IsContainerTrackingDisposable { get; }
        object GetInstance(DiContainer container);
    }
}