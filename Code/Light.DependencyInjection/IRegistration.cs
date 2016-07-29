namespace Light.DependencyInjection
{
    public interface IRegistration
    {
        object Create(DiContainer container);
    }
}