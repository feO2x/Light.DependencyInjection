using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.DependencyInjection.Registrations
{
    public static class RegistrationAssertions
    {
        public static Type MustBeBaseTypeOf(this Type baseClassOrInterfaceType, Type type, IEqualityComparer<Type> typeComparer = null)
        {
            if (type.IsDerivingFromOrImplementing(baseClassOrInterfaceType, typeComparer))
                return baseClassOrInterfaceType;

            throw new RegistrationException($"Type \"{baseClassOrInterfaceType}\" cannot be used as an abstraction for type \"{type}\" because the latter type does not derive from or implement the former one.", type);
        }

        public static IReadOnlyList<Dependency> VerifyDependencies(this IReadOnlyList<Dependency> dependencies, ParameterInfo[] parameters, Type targetType, string targetName = null)
        {
            targetName = targetName ?? "target";

            if (dependencies == null)
            {
                if (parameters.Length > 0)
                    throw new RegistrationException(new StringBuilder().AppendCollectionContent(parameters, $"There are no dependencies specified although the {targetName} has the following parameters:")
                                                                       .ToString(),
                                                    targetType);
            }

            else if (dependencies.Count != parameters.Length ||
                     dependencies.Select(dependency => dependency.DependencyType).SequenceEqual(parameters.Select(parameter => parameter.ParameterType)) == false)
                throw new RegistrationException(new StringBuilder().AppendCollectionContent(dependencies, "The following dependencies")
                                                                   .AppendLine()
                                                                   .AppendCollectionContent(parameters, $"do not correspond to the following parameters of the {targetName}:")
                                                                   .ToString(),
                                                targetType);

            return dependencies;
        }

        public static TypeKey MustBeValidRegistrationTypeKey(this TypeKey typeKey)
        {
            typeKey.MustNotBeEmpty(nameof(typeKey)).Type.MustBeValidRegistrationType();
            return typeKey;
        }

        public static Type MustBeValidRegistrationType(this Type type)
        {
            type.MustNotBeNull(nameof(type));

            if (type.IsGenericParameter)
                throw new RegistrationException($"You cannot register the generic type parameter \"{type}\" with the DI Container. For generic types, only closed constructed generic types and generic type definitions are allowed.", type);
            if (type.IsStaticClass())
                throw new RegistrationException($"You cannot register the static class \"{type}\" with the DI Container.", type);
            if (type.IsOpenConstructedGenericType())
                throw new RegistrationException($"You cannot register the open constructed generic type \"{type}\" with the DI Container. For generic types, only closed constructed generic types and generic type definitions are allowed.", type);

            return type;
        }
    }
}