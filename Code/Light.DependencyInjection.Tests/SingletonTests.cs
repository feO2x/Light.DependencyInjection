using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class SingletonTests
    {
        [Fact]
        public void ResolveSingleton()
        {
            var diContainer = new DiContainer();
            diContainer.RegisterSingleton<Dummy>();

            var first = diContainer.Resolve<Dummy>();
            var second = diContainer.Resolve<Dummy>();

            first.Should().BeSameAs(second);
        }

        public class Dummy { }
    }
}