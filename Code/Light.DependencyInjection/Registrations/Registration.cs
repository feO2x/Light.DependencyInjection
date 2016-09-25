using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.TypeConstruction;

namespace Light.DependencyInjection.Registrations
{
    public sealed class Registration : BaseRegistration
    {
        public Registration(TypeKey typeKey,
                            ILifetime lifetime,
                            TypeCreationInfo typeCreationInfo = null,
                            bool isTrackingDisposables = true)
            : base(typeKey, lifetime, typeCreationInfo, isTrackingDisposables)
        {
            TargetType.MustBeInstantiatable();
        }

        public ILifetime Lifetime => _lifetime;
    }
}