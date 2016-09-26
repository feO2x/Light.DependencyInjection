using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class PerResolveLifetimeTests : DefaultDiContainerTests
    {
        [Fact(DisplayName = "The PerResolveLifetime must inject the same instance during a recursive run of DiContainer.Resolve.")]
        public void SameInstancePerResolve()
        {
            Container.RegisterPerResolve<A>()
                     .RegisterInstance(42);

            var instanceOfL = Container.Resolve<L>();

            instanceOfL.ReferenceToA.Should().BeSameAs(instanceOfL.ReferenceToB.OtherObject);
        }

        [Fact(DisplayName = "The PerResolveLifetime must return different instances for different calls to DiContainer.Resolve.")]
        public void DifferentInstancesOnDifferentResolves()
        {
            Container.RegisterPerResolve<A>();

            var firstC = Container.Resolve<C>();
            var secondC = Container.Resolve<C>();

            firstC.ReferenceToA.Should().NotBeSameAs(secondC.ReferenceToA);
        }
    }
}