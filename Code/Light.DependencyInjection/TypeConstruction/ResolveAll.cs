using System;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Services;

namespace Light.DependencyInjection.TypeConstruction
{
    public static class ResolveAll
    {
        public static IDependencyResolver Create(Type requestedType)
        {
            var constructor = typeof(ResolveAll<>).MakeGenericType(requestedType).GetDefaultConstructor();
            return (IDependencyResolver) constructor.Invoke(null);
        }
    }

    public sealed class ResolveAll<T> : IDependencyResolver
    {
        public object Resolve(Type type, string registrationName, CreationContext context)
        {
            return context.Container.ResolveAll<T>();
        }
    }
}