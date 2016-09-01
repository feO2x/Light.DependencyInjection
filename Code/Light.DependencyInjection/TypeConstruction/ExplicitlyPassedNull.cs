namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ExplicitlyPassedNull
    {
        public static readonly ExplicitlyPassedNull Instance = new ExplicitlyPassedNull();

        private ExplicitlyPassedNull() { }
    }
}