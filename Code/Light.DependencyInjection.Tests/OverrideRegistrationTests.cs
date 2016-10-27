using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class OverrideRegistrationTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "Clients must be able to override a registration for a single resolve.")]
        public void OverrideRegistration()
        {
            Container.RegisterTransient<L>()
                     .RegisterTransient<B>()
                     .RegisterSingleton<A>()
                     .RegisterInstance(42);

            var otherInstanceOfA = new A();
            var instanceOfL = Container.OverrideMapping(otherInstanceOfA)
                                       .Resolve<L>();

            instanceOfL.ReferenceToA.Should().BeSameAs(otherInstanceOfA);
            instanceOfL.ReferenceToB.ReferenceToA.Should().BeSameAs(otherInstanceOfA);
        }

        [Fact(DisplayName = "Clients must be able to override an abstract type to concrete type mapping, even when the concrete types differ.")]
        public void OverrideAbstraction()
        {
            IC otherC = new OtherC();
            Container.RegisterSingleton<C>(options => options.MapToAbstractions(typeof(IC)))
                     .OverrideMapping(otherC);

            
            var instance = Container.Resolve<IC>();

            instance.Should().BeSameAs(otherC);
        }

        public sealed class OtherC : IC { }
    }
}