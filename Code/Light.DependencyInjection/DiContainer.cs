using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection
{
    public sealed class DiContainer
    {
        private readonly IDictionary<TypeKey, ICreationDescription> _mappings;

        public DiContainer() : this(new Dictionary<TypeKey, ICreationDescription>()) { }

        public DiContainer(IDictionary<TypeKey, ICreationDescription> mappings)
        {
            mappings.MustNotBeNull(nameof(mappings));

            _mappings = mappings;
        }

        public DiContainer RegisterSingleton<T>()
        {
            _mappings.Add(new TypeKey(typeof(T)), new Singleton(typeof(T)));
            return this;
        }

        public T Resolve<T>()
        {
            var type = typeof(T);
            var typeKey = new TypeKey(type);
            ICreationDescription creationDescription;
            if (_mappings.TryGetValue(typeKey, out creationDescription) == false)
            {
                creationDescription = new Transient(typeof(T));
                _mappings.Add(typeKey, creationDescription);
            }

            return (T) creationDescription.Create(this);
        }
    }
}