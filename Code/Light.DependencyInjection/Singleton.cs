using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class Singleton : ICreationDescription
    {
        private readonly Type _targetType;
        private object _instance;

        public Singleton(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            _targetType = targetType;
        }

        public object Create(DiContainer container)
        {
            if (_instance != null)
                return _instance;

            _instance = Activator.CreateInstance(_targetType);
            return _instance;
        }
    }
}