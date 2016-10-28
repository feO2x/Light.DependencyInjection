using System;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class DefaultDependencyResolver : IDependencyResolver
    {
        public static readonly DefaultDependencyResolver Instance = new DefaultDependencyResolver();

        public object Resolve(Type type, string registrationName, ResolveContext context)
        {
            return context.ResolveChildValue(new TypeKey(type, registrationName));
        }
    }
}