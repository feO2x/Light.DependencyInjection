using System;
using System.Collections.Generic;

namespace Light.DependencyInjection.TypeResolving
{
    public static class KnownTypes
    {
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type ResolveContextType = typeof(ResolveContext);
        public static readonly Type DependencyOverridesType = typeof(DependencyOverrides);

        // ReSharper disable InconsistentNaming
        public static readonly Type IEnumerableGenericTypeDefinition = typeof(IEnumerable<>);

        public static readonly Type ICollectionGenericTypeDefinition = typeof(ICollection<>);
        // ReSharper restore InconsistentNaming
    }
}