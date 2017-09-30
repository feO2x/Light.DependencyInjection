using System;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ChildContainerTests
    {
        [Fact(DisplayName = "A child container must return the scoped instances of the parent container when possible.")]
        public void AccessParentScopedInstances()
        {
            var container = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseScopedLifetime());
            var scopedInstanceFromParentContainer = container.Resolve<ClassWithoutDependencies>();

            var childContainer = container.CreateChildContainer();
            var scopedInstanceFromChildContainer = childContainer.Resolve<ClassWithoutDependencies>();

            scopedInstanceFromChildContainer.Should().BeSameAs(scopedInstanceFromParentContainer);
        }

        [Theory(DisplayName = "A child container must create its own scoped instance when the parent container has none available.")]
        [MemberData(nameof(DifferentScopedInstancesData))]
        public void DifferentScopedInstances(Action<IRegistrationOptions<ClassWithoutDependencies>> configureLifetime)
        {
            var parentContainer = new DiContainer().Register(configureLifetime);

            var childContainer1 = parentContainer.CreateChildContainer();
            var instanceOf1 = childContainer1.Resolve<ClassWithoutDependencies>();
            var childContainer2 = parentContainer.CreateChildContainer();
            var instanceOf2 = childContainer2.Resolve<ClassWithoutDependencies>();

            instanceOf1.Should().NotBeSameAs(instanceOf2);
        }

        public static readonly TestData DifferentScopedInstancesData =
            new[]
            {
                new object[]
                {
                    new Action<IRegistrationOptions<ClassWithoutDependencies>>(options => options.UseScopedLifetime())
                },
                new object[]
                {
                    new Action<IRegistrationOptions<ClassWithoutDependencies>>(options => options.UseHierarchicalScopedLifetime())
                }
            };

        [Fact(DisplayName = "A child container must not return the scoped instances of the parent when a HierarchicalScopedLifetime is used.")]
        public void DoNotAccessParentScopedInstancesForHierarchicalScopedLifetimeRegistrations()
        {
            var parentContainer = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseHierarchicalScopedLifetime());
            var parentInstance1 = parentContainer.Resolve<ClassWithoutDependencies>();
            var parentInstance2 = parentContainer.Resolve<ClassWithoutDependencies>();
            parentInstance1.Should().BeSameAs(parentInstance2);

            var childContainer = parentContainer.CreateChildContainer();
            var childInstance1 = childContainer.Resolve<ClassWithoutDependencies>();
            var childInstance2 = childContainer.Resolve<ClassWithoutDependencies>();

            childInstance1.Should().BeSameAs(childInstance2);
            childInstance1.Should().NotBeSameAs(parentInstance1);
        }

        [Fact(DisplayName = "The registrations of a child container can be detached from its parent container.")]
        public void DetachedChildContainer()
        {
            var containerServices = new ContainerServicesBuilder().WithAutomaticRegistrationFactory(new NoAutoRegistrationsAllowedFactory()).Build();
            var parentContainer = new DiContainer(containerServices);

            var childContainer = parentContainer.CreateChildContainer(new ChildContainerOptions(true));
            var instance = childContainer.Register<ClassWithoutDependencies>().Resolve<ClassWithoutDependencies>();

            instance.Should().NotBeNull();
            parentContainer.TryGetRegistration<ClassWithoutDependencies>().Should().BeNull();
        }

        [Fact(DisplayName = "The DI Container must be able to store external instances in the container scope.")]
        public void ScopedExternalInstanceLifetime()
        {
            var containerServices = new ContainerServicesBuilder().WithAutomaticRegistrationFactory(new NoAutoRegistrationsAllowedFactory()).Build();
            var parentContainer = new DiContainer(containerServices).PrepareScopedExternalInstance<DisposableSpy>();
            var externalSpy = new DisposableSpy();
            var childContainer = parentContainer.CreateChildContainer();
            childContainer.AddPreparedExternalInstanceToScope(externalSpy);

            var resolvedInstance = childContainer.Resolve<DisposableSpy>();
            resolvedInstance.Should().BeSameAs(externalSpy);

            childContainer.Dispose();
            externalSpy.DisposeMustHaveBeenCalledExactlyOnce();
        }
    }
}