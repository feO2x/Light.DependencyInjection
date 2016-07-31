using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests
    {
        private readonly DiContainer _container = new DiContainer();

        [Fact(DisplayName = "The DI container must return one and the same instance when a type is registered as a Singleton.")]
        public void ResolveSingleton()
        {
            _container.RegisterSingleton<A>();

            var first = _container.Resolve<A>();
            var second = _container.Resolve<A>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must create a transient registration and use it  when resolve is called for a non-registered type.")]
        public void ResolveDefaultTransient()
        {
            _container.Resolve<A>();

            _container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration is TransientRegistration);
        }

    }
}