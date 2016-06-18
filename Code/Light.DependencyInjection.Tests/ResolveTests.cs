using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests
    {
        [Fact]
        public void ResolveSingleton()
        {
            var diContainer = new DiContainer();
            diContainer.RegisterSingleton<Dummy>();

            var first = diContainer.Resolve<Dummy>();
            var second = diContainer.Resolve<Dummy>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void ResolveTransient()
        {
            var diContainer = new DiContainer();

            var first = diContainer.Resolve<Dummy>();
            var second = diContainer.Resolve<Dummy>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        public class Dummy { }
    }
}