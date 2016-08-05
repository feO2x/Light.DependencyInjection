namespace Light.DependencyInjection
{
    public interface IRegistrationOptions
    {
        IRegistrationOptions WithRegistrationName(string registrationName);
        IRegistrationOptions UseDefaultConstructor();
        IRegistrationOptions UseConstructorWithParameter<TParameter>();
        IRegistrationOptions UseConstructorWithParameters<T1, T2>();
    }
}