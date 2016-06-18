namespace Light.DependencyInjection
{
    public interface ICreationDescription
    {
        object Create(DiContainer container);
    }
}