using Xunit;
using FluentAssertions;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class BasicResolveTests
    {
        [Fact(DisplayName = "The DI Container must create two instances when a type with a transient lifetime is resolved two times.")]
        public void TransientResolve()
        {
            var container = new DiContainer().RegisterTransient<ClassWithoutDependencies>();

            var first = container.Resolve<ClassWithoutDependencies>();
            var second = container.Resolve<ClassWithoutDependencies>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact(DisplayName = "The DI Container must resolve the same instance when the target type is registered with a singleton lifetime.")]
        public void SingletonResolve()
        {
            var container = new DiContainer().RegisterSingleton<ClassWithoutDependencies>();

            var first = container.Resolve<ClassWithoutDependencies>();
            var second = container.Resolve<ClassWithoutDependencies>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }
    }
}
