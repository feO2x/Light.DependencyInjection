using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.Services;
using Light.DependencyInjection.TypeResolving;
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

        [Fact(DisplayName = "The DI Container should not perform a automatic ResolveAll for nested dependencies when the default IResolveInfoAlgorithm is replaced.")]
        public void DisableResolveAllByDefault()
        {
            var containerServices = new ContainerServicesBuilder().WithResolveInfoAlgorithm(new ResolveOnlyRegistrationsAlgorithm()).Build();
            var concreteTypes = new[] { typeof(Implementation2), typeof(Implementation3), typeof(Implementation1) };
            var container = new DiContainer(containerServices).RegisterMany<IAbstractionA>(concreteTypes)
                                                              .Register<ClassWithCollectionDependency>();

            var resolvedInstance = container.Resolve<ClassWithCollectionDependency>();

            resolvedInstance.Instances.Should().HaveCount(0).And.Subject.Should().BeAssignableTo<List<IAbstractionA>>();
        }

        [Fact(DisplayName = "The DI Container must allow clients to explicitly set ResolveAll for instantiation dependencies by type.")]
        public void ExplicitelyEnableResolveAllForInstantiationDependency()
        {
            var containerServices = new ContainerServicesBuilder().WithResolveInfoAlgorithm(new ResolveOnlyRegistrationsAlgorithm()).Build();
            var concreteTypes = new[] { typeof(Implementation1), typeof(Implementation3), typeof(Implementation2) };
            var container = new DiContainer(containerServices).RegisterMany<IAbstractionA>(concreteTypes)
                                                              .Register<ClassWithCollectionDependency>(options => options.ConfigureInstantiationParameter<IReadOnlyList<IAbstractionA>>(parameter => parameter.SetResolveAllTo(true)));

            var resolvedInstance = container.Resolve<ClassWithCollectionDependency>();

            resolvedInstance.Instances.Should().HaveCount(3);
            for (var i = 0; i < resolvedInstance.Instances.Count; i++)
            {
                resolvedInstance.Instances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }

        [Fact(DisplayName = "The DI Container must allow clients to explicitly set ResolveAll for instantiation dependencies by name.")]
        public void ExplicitelyResolveAllForInstantiationDependencyByName()
        {
            var containerServices = new ContainerServicesBuilder().WithResolveInfoAlgorithm(new ResolveOnlyRegistrationsAlgorithm()).Build();
            var concreteTypes = new[] { typeof(Implementation1), typeof(Implementation3), typeof(Implementation2) };
            var container = new DiContainer(containerServices).RegisterMany<IAbstractionA>(concreteTypes)
                                                              .Register<ClassWithCollectionDependency>(options => options.ConfigureInstantiationParameter("instances", parameter => parameter.SetResolveAllTo(true)));

            var resolvedInstance = container.Resolve<ClassWithCollectionDependency>();

            resolvedInstance.Instances.Should().HaveCount(3);
            for (var i = 0; i < resolvedInstance.Instances.Count; i++)
            {
                resolvedInstance.Instances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }

        [Fact(DisplayName = "The DI Container must allow clients to explicitly set ResolveAll for property injections.")]
        public void ExplicitelyResolveAllForPropertyInjection()
        {
            var containerServices = new ContainerServicesBuilder().WithResolveInfoAlgorithm(new ResolveOnlyRegistrationsAlgorithm()).Build();
            var concreteTypes = new[] { typeof(Implementation1), typeof(Implementation3), typeof(Implementation2) };
            var container = new DiContainer(containerServices).RegisterMany<IAbstractionA>(concreteTypes)
                                                              .Register<ClassWithCollectionDependencyOnProperty>(options => options.AddPropertyInjection(nameof(ClassWithCollectionDependencyOnProperty.Instances), dependency => dependency.SetResolveAllTo(true)));

            var resolvedInstance = container.Resolve<ClassWithCollectionDependencyOnProperty>();

            resolvedInstance.Instances.Should().HaveCount(3);
            for (var i = 0; i < resolvedInstance.Instances.Count; i++)
            {
                resolvedInstance.Instances[i].GetType().Should().Be(concreteTypes[i]);
            }
        }
    }
}