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

        [Fact(DisplayName = "The DI container must return new instances when a type is registered as with a transient lifetime.")]
        public void ResolveTransient()
        {
            _container.RegisterTransient<A>();

            var first = _container.Resolve<A>();
            var second = _container.Resolve<A>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a type depending on other objects / values. These values must be resolved recursively.")]
        public void ResolveRecursively()
        {
            _container.RegisterSingleton<A>()
                      .RegisterTransient<C>();

            var firstC = _container.Resolve<C>();
            var secondC = _container.Resolve<C>();

            firstC.Should().NotBeSameAs(secondC);
            firstC.ReferenceToA.Should().BeSameAs(secondC.ReferenceToA);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a concrete type for a polymorphic abstraction when this mapping was registered with it beforehand.")]
        public void AbstractionMapping()
        {
            _container.RegisterSingleton<A>()
                      .RegisterTransient<IC, C>();

            var interfaceReference = _container.Resolve<IC>();

            interfaceReference.Should().BeOfType<C>();
        }

        [Fact]
        public void RegisterInstance()
        {
            var instance = new A();
            _container.RegisterInstance(instance);

            var resolvedInstance = _container.Resolve<A>();

            resolvedInstance.Should().BeSameAs(instance);
        }

        [Fact(DisplayName = "The DI container must create a transient registration and use it  when Resolve is called for a non-registered type.")]
        public void ResolveDefaultTransient()
        {
            _container.Resolve<A>();

            _container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration is TransientRegistration);
        }
    }
}