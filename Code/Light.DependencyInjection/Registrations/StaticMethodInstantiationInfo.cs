using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class StaticMethodInstantiationInfo : InstantiationInfo
    {
        public readonly MethodInfo StaticMethod;

        public StaticMethodInstantiationInfo(TypeKey typeKey,
                                                 MethodInfo staticMethod,
                                                 IReadOnlyList<Dependency> instantiationDependencies)
            : base(typeKey, instantiationDependencies)
        {
            staticMethod.MustNotBeNull(nameof(staticMethod));
            if (staticMethod.IsStatic == false)
                throw new ArgumentException($"The specified method \"{staticMethod}\" is not static.");

            StaticMethod = staticMethod;
        }
    }
}