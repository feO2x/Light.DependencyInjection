using System;

namespace Light.DependencyInjection.TypeResolving
{
    public static class KnownTypes
    {
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type DiContainerType = typeof(DiContainer);
        public static readonly Type ResolveContextType = typeof(ResolveContext);
    }
}