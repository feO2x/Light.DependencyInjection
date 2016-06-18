using System;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class Transient : ICreationDescription
    {
        private readonly Type _type;

        public Transient(Type type)
        {
            type.MustNotBeNull(nameof(type));

            _type = type;
        }

        public object Create(DiContainer container)
        {
            return Activator.CreateInstance(_type);
        }
    }
}