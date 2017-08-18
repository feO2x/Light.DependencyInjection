using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ErrorMessagesTests
    {
        [Fact(DisplayName = "The DI Container must throw a RegistrationException when the single specified type is a generic type definition.")]
        public void GenericParameterTest()
        {
            var container = new DiContainer();
            var genericTypeParameter = typeof(List<>).GetTypeInfo().GenericTypeParameters[0];

            Action act = () => container.Register(genericTypeParameter);

            act.ShouldThrow<RegistrationException>()
               .And.Message.Should().Contain($"You cannot register the generic type parameter \"{genericTypeParameter}\" with the DI Container.");
        }
    }
}