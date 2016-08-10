using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class SingletonRegistration : Registration
    {
        private object _instance;

        public SingletonRegistration(TypeCreationInfo typeCreationInfo, string registrationName = null) : base(typeCreationInfo, registrationName) { }

        public override object GetInstance(DiContainer container)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = TypeCreationInfo.CreateInstance(container);
                }
            }
            return _instance;
        }
    }
}