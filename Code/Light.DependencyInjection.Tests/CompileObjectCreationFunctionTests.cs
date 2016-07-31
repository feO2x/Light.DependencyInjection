using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.GuardClauses;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class CompileObjectCreationFunctionTests
    {
        [Fact(DisplayName = "CompileObjectCreationFunction creates a dynamic delegate for a constructor with parameters that accepts an object array and calls the target constructor using the array's items as parameter.")]
        public void ConstructorWithParameters()
        {
            var compiledCreationFunction = typeof(B).GetTypeInfo()
                                                    .DeclaredConstructors
                                                    .First()
                                                    .CompileObjectCreationFunction();

            var parameters = new object[]
                             {
                                 new A(),
                                 42
                             };

            var createdObject = compiledCreationFunction(parameters);

            var instanceOfB = createdObject.MustBeOfType<B>();
            instanceOfB.OtherObject.Should().BeSameAs(parameters[0]);
            instanceOfB.Value.Should().Be(42);
        }

        [Fact(DisplayName = "CompileObjectCreationFunction creates a dynamic delegate for a default constructor where the object array used as a parameter can be passed in as null.")]
        public void DefaultConstructor()
        {
            var compiledCreationFunction = typeof(A).GetTypeInfo()
                                                    .DeclaredConstructors
                                                    .First()
                                                    .CompileObjectCreationFunction();

            var createdObject = compiledCreationFunction(null);

            createdObject.Should().BeOfType<A>();
        }

        
    }
}