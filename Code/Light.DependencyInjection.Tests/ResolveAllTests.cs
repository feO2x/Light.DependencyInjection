using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ResolveAllTests
    {
        [Fact(DisplayName = "The DI Container must be able to resolve all registrations of a certain type.")]
        public void ResolveAll()
        {
            var container = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Foo"))
                                             .Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Bar"))
                                             .Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Baz"));

            var resolvedInstance = container.ResolveAll<ClassWithoutDependencies>();

            resolvedInstance.Should().HaveCount(3);
            resolvedInstance.Should().NotContainNulls();
        }
    }
}