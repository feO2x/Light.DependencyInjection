using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.Registrations
{
    public sealed class ConstructorWithMostParametersSelector : IDefaultInstantiationInfoSelector
    {
        public InstantiationInfoFactory GetDefaultInstantiationInfo(TypeInfo typeInfo)
        {
            typeInfo.MustNotBeNull(nameof(typeInfo));

            var constructorsWithMostParameters = typeInfo.DeclaredConstructors
                                                         .Where(constructor => constructor.IsPublic && constructor.IsStatic == false)
                                                         .GroupBy(constructor => constructor.GetParameters().Length)
                                                         .OrderByDescending(group => group.Key)
                                                         .FirstOrDefault();
            CheckConstructorGroup(constructorsWithMostParameters, typeInfo);

            return new ConstructorInstantiationInfoFactory(constructorsWithMostParameters.First());
        }

        private static void CheckConstructorGroup(IEnumerable<ConstructorInfo> constructorGroup, TypeInfo typeInfo)
        {
            var type = typeInfo.AsType();
            if (constructorGroup == null)
                throw new RegistrationException($"Cannot register \"{type}\" with the DI container because this type does not contain a public non-static constructor. Please specify an instantiation method using the registration options.", type);

            if (constructorGroup.Count() == 1)
                return;

            throw new RegistrationException($"Cannot register \"{type}\" with the DI container because this type contains two or more constructors with the same number of parameters. Please specify an instantiation method using the registration options.", type); // TODO: append the constructors to the exception message
        }
    }
}