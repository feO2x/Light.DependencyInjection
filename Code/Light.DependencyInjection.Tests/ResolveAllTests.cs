using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ResolveAllTests
    {
        [Fact(DisplayName = "The DI Container must be able to resolve all registrations of a concrete type.")]
        public void ResolveAll()
        {
            var container = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Foo"))
                                             .Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Bar"))
                                             .Register<ClassWithoutDependencies>(options => options.UseRegistrationName("Baz"));

            var resolvedInstance = container.ResolveAll<ClassWithoutDependencies>();

            resolvedInstance.Should().HaveCount(3);
            resolvedInstance.Should().NotContainNulls();
        }

        [Fact(DisplayName = "The DI Container must be able to resolve all registrations of an abstraction type in the same order as they were registered.")]
        public void ResolveAllViaAbstraction()
        {
            var concreteTypes = new[] { typeof(Implementation1), typeof(Implementation2), typeof(Implementation3) };
            var container = new DiContainer().RegisterMany<IAbstractionA>(concreteTypes);

            var instances = container.ResolveAll<IAbstractionA>();

            instances.Count.Should().Be(concreteTypes.Length);
            for (var i = 0; i < instances.Count; i++)
            {
                instances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }

        [Fact(DisplayName = "The DI Container must be able to resolve all registrations as part of a complex object graph.")]
        public void ResolveAllHierarchically()
        {
            var concreteTypes = new[] { typeof(Implementation2), typeof(Implementation3), typeof(Implementation1) };
            var container = new DiContainer().RegisterMany<IAbstractionA>(concreteTypes)
                                             .Register<ClassWithCollectionDependency>();

            var resolvedInstance = container.Resolve<ClassWithCollectionDependency>();

            resolvedInstance.Instances.Should().HaveCount(3);
            for (var i = 0; i < resolvedInstance.Instances.Count; i++)
            {
                resolvedInstance.Instances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }

        [Fact(DisplayName = "The DI Container should perform a Resolve All by default when a collection of abstraction instances is requested.")]
        public void ResolveAllViaStandardResolve()
        {
            var concreteTypes = new[] { typeof(Implementation3), typeof(Implementation2), typeof(Implementation1) };
            var container = new DiContainer().RegisterMany<IAbstractionA>(concreteTypes);

            var resolvedInstances = container.Resolve<IReadOnlyList<IAbstractionA>>();

            resolvedInstances.Should().HaveCount(3);
            for (var i = 0; i < resolvedInstances.Count; i++)
            {
                resolvedInstances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }
    }
}