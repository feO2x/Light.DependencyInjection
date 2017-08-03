using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Xunit;

namespace Light.DependencyInjection.Tests
{
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

        [Fact(DisplayName = "A child container must create its own scoped instance when the parent container has none available.")]
        public void DifferentScopedInstances()
        {
            var parentContainer = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseScopedLifetime());

            var childContainer1 = parentContainer.CreateChildContainer();
            var instanceOf1 = childContainer1.Resolve<ClassWithoutDependencies>();
            var childContainer2 = parentContainer.CreateChildContainer();
            var instanceOf2 = childContainer2.Resolve<ClassWithoutDependencies>();

            instanceOf1.Should().NotBeSameAs(instanceOf2);
        }
    }
}