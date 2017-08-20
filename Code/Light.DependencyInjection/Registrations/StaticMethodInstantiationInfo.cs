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
                throw new RegistrationException($"The specified method \"{staticMethod}\" is not static.");

            if (staticMethod.ReturnParameter.ParameterType.IsInInheritanceHierarchyOf(TypeKey.Type) == false)
                throw new RegistrationException($"The return type \"{staticMethod.ReturnParameter.ParameterType}\" of the static method \"{staticMethod}\" is not in the inheritance hierarchy of the target type \"{typeKey.Type}\".");

            if (typeKey.Type.IsGenericTypeDefinition() && staticMethod.ReturnType.IsOpenConstructedGenericType() == false)
                throw new RegistrationException($"You cannot instantiate type \"{typeKey.Type}\" with the static factory method \"{staticMethod}\". This type is a generic type definition, thus your factory method must return a open constructed generic type that resides in the same inheritance hierarchy as the registration type.");

            instantiationDependencies.VerifyDependencies(staticMethod.GetParameters(), "static method");

            StaticMethod = staticMethod;
        }
    }
}