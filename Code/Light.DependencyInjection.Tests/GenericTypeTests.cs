using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class GenericTypeTests
    {
        [Fact(DisplayName = "The DI Container must be able to perform registrations and resolves for closed bound generic types.")]
        public void ClosedBoundGenericTypes()
        {
            var container = new DiContainer().Register<IList<string>, List<string>>(options => options.UseDefaultConstructor());

            var instance = container.Resolve<IList<string>>();

            instance.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to perform registrations of generic type definitions and resolve bound closed instances of it.")]
        public void GenericTypeDefinitions()
        {
            var container = new DiContainer().Register(typeof(List<>), options => options.UseDefaultConstructor()
                                                                                         .MapToAbstractions(typeof(IList<>)));

            var instance = container.Resolve<IList<string>>();

            instance.Should().BeAssignableTo<List<string>>();
        }
    }
}