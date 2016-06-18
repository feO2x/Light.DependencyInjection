using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class SimpleResolveTests
    {
        private readonly DiContainer _container = new DiContainer();

        [Fact]
        public void ResolveSingleton()
        {
            _container.RegisterSingleton<Dummy>();

            var first = _container.Resolve<Dummy>();
            var second = _container.Resolve<Dummy>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Fact]
        public void ResolveTransient()
        {
            var first = _container.Resolve<Dummy>();
            var second = _container.Resolve<Dummy>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        public class Dummy { }
    }
}