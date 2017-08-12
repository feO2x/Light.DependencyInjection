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
                throw new RegistrationException($"The specified method \"{staticMethod}\" is not static.", TypeKey.Type);

            if (staticMethod.ReturnParameter.ParameterType.IsInInheritanceHierarchyOf(TypeKey.Type) == false)
                throw new RegistrationException($"The return type \"{staticMethod.ReturnParameter.ParameterType}\" of the static method \"{staticMethod}\" is not in the inheritance hierarchy of the target type \"{typeKey.Type}\".", TypeKey.Type);

            instantiationDependencies.VerifyDependencies(staticMethod.GetParameters(), TypeKey.Type, "static method");

            StaticMethod = staticMethod;
        }
    }
}