using System;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class OverrideMappingTests : DefaultDependencyInjectionContainerTest
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

        [Fact(DisplayName = "Clients must be able to override an abstract-type-to-concrete-type-mapping, even when the concrete types differ.")]
        public void OverrideAbstraction()
        {
            IC otherC = new OtherC();
            Container.RegisterSingleton<C>(options => options.MapToAbstractions(typeof(IC)))
                     .OverrideMapping(otherC);


            var instance = Container.Resolve<IC>();

            instance.Should().BeSameAs(otherC);
        }

        [Fact(DisplayName = "Clients must be able to override a registration whose instances is accessed through an abstraction.")]
        public void OverrideRegistrationAccessedByAbstraction()
        {
            var overridenC = new C(new A());
            Container.RegisterTransient<M>()
                     .RegisterSingleton<IC, C>()
                     .OverrideMapping(overridenC);

            var instanceOfM = Container.Resolve<M>();

            instanceOfM.First.Should().BeSameAs(overridenC);
            instanceOfM.Second.Should().BeSameAs(overridenC);
        }

        [Fact(DisplayName = "Clients must be able to override an abstract-type-to-concrete-type-mapping with a non-generic API.")]
        public void OverrideAbstractionNonGeneric()
        {
            var otherC = new OtherC();
            Container.RegisterSingleton<C>(options => options.MapToAllImplementedInterfaces())
                     .OverrideMapping(otherC, typeof(IC));

            var instance = Container.Resolve<IC>();

            instance.Should().BeSameAs(otherC);
        }

        [Fact(DisplayName = "The non-generic variant of OverrideMapping must throw a TypeRegistrationException when the provided type is not a base type of the specified instance.")]
        public void OverrideAbstractionError()
        {
            Action act = () => Container.OverrideMapping(new A(), typeof(IC));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The concrete type \"{typeof(A)}\" does not inherit from or implement type \"{typeof(IC)}\".");
        }

        public sealed class OtherC : IC { }
    }
}