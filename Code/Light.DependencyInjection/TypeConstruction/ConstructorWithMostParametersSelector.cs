using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Light.GuardClauses;

namespace Light.DependencyInjection.TypeConstruction
{
    public sealed class ConstructorWithMostParametersSelector : IConstructorSelector
    {
        public ConstructorInfo SelectTargetConstructor(TypeInfo typeInfo)
        {
            if (typeInfo.DeclaredConstructors.Count() == 1)
                return typeInfo.DeclaredConstructors.First();

            var constructorsWithMostParameters = typeInfo.DeclaredConstructors
                                                         .GroupBy(constructor => constructor.GetParameters().Length)
                                                         .OrderByDescending(group => group.Key)
                                                         .First();
            CheckConstructorGroup(constructorsWithMostParameters, typeInfo);

            return constructorsWithMostParameters.First();
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void CheckConstructorGroup(IEnumerable<ConstructorInfo> constructorGroup, TypeInfo typeInfo)
        {
            if (constructorGroup.Count() == 1)
                return;

            var type = typeInfo.AsType();
            throw new TypeRegistrationException($"Cannot register \"{type}\" with the DI container because this type contains two or more constructors with the same number of parameters.", type);  // TODO: append the constructors to the exception message
        }
    }
}