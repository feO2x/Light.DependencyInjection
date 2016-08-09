using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class SingletonRegistration : Registration
    {
        private object _instance;

        public SingletonRegistration(TypeInstantiationInfo typeInstantiationInfo, string registrationName = null) : base(typeInstantiationInfo, registrationName) { }

        public override object GetInstance(DiContainer container)
        {
            if (_instance == null)
            {
                lock (this)
                {
                    if (_instance == null)
                        _instance = TypeInstantiationInfo.InstantiateObjectAndPerformInstanceInjections(container);
                }
            }
            return _instance;
        }
    }
}