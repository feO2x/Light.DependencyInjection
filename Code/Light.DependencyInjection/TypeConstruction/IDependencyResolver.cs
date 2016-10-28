using System;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    public interface IDependencyResolver
    {
        object Resolve(Type type, string registrationName, ResolveContext context);
    }
}