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
    }
}