using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class RegistrationOverrideTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "Clients must be able to override a registration for a single resolve.")]
        public void OverrideRegistration()
        {
            Container.RegisterTransient<L>()
                     .RegisterTransient<B>()
                     .RegisterSingleton<A>()
                     .RegisterInstance(42);

            var otherInstanceOfA = new A();
            var instanceOfL = Container.WithRegistrationOverride(otherInstanceOfA)
                                       .Resolve<L>();

            instanceOfL.ReferenceToA.Should().BeSameAs(otherInstanceOfA);
            instanceOfL.ReferenceToB.ReferenceToA.Should().BeSameAs(otherInstanceOfA);
        }
    }
}