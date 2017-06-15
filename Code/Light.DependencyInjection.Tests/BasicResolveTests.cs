using Xunit;
using FluentAssertions;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

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

        [Theory(DisplayName = "The DI Container must resolve the reference to an instance that was passed to it on registration.")]
        [MemberData(nameof(ExternalInstanceData))]
        public void ExternalInstanceResolve<T>(T instance)
        {
            var container = new DiContainer().RegisterInstance(instance);

            var resolvedInstance = container.Resolve<T>();

            resolvedInstance.Should().BeSameAs(instance);
        }

        public static readonly TestData ExternalInstanceData =
            new[]
            {
                new object[] { "Foo" },
                new[] { new object() }
            };

        [Fact(DisplayName = "The DI container must be able to resolve types with dependencies to other types.")]
        public void SimpleHierarchicalResolve()
        {
            var container = new DiContainer().RegisterTransient<ClassWithoutDependencies>()
                                             .RegisterTransient<ClassWithDependency>();

            var instance = container.Resolve<ClassWithDependency>();

            instance.Should().NotBeNull();
            instance.A.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI container must be able to resolve a complex object graph where a singleton instance is injected in several other objects.")]
        public void TwoLevelHierarchicalResolveWithSingletonLeaf()
        {
            var container = new DiContainer().RegisterSingleton<ClassWithoutDependencies>()
                                             .RegisterTransient<ClassWithDependency>()
                                             .RegisterSingleton<ClassWithTwoDependencies>();

            var instance = container.Resolve<ClassWithTwoDependencies>();

            instance.A.Should().BeSameAs(instance.B.A);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a concrete type for an interface when this mapping was registered beforehand.")]
        public void InterfaceMapping()
        {
            var instance = new DiContainer().RegisterSingleton<IAbstractionA, ClassWithoutDependencies>()
                                            .Resolve<IAbstractionA>();

            instance.Should().BeOfType<ClassWithoutDependencies>();
        }
    }
}
