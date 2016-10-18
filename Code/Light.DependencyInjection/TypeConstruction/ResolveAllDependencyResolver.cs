using System;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ResolveAllDependencyResolver<T> : IDependencyResolver
    {
        public object Resolve(Type type, string registrationName, CreationContext context)
        {
            return context.Container.ResolveAll<T>();
        }
    }
}