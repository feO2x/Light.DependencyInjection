using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ParameterOverridesTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "Clients must be able to override instantiation values using the parameter name.")]
        public void OverrideInstantiationParameterByName()
        {
            Container.RegisterTransient<B>()
                     .RegisterTransient<A>()
                     .RegisterInstance(42);

            var parameterOverrides = Container.OverrideParametersFor<B>().OverrideInstantiationParameter("value", 87);
            var instanceOfB = Container.Resolve<B>(parameterOverrides);

            instanceOfB.Value.Should().Be(87);
        }

        [Fact(DisplayName = "Clients must be able to override instantiation values using the parameter type.")]
        public void OverridInstantiationParameterByType()
        {
            Container.RegisterTransient<D>()
                     .RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor()
                                                                          .MapToAbstractions(typeof(IList<>)));

            var parameterOverrides = Container.OverrideParametersFor<D>().OverrideInstantiationParameter<int>(42);
            var instanceOfD = Container.Resolve<D>(parameterOverrides);

            instanceOfD.SomeNumber.Should().Be(42);
        }

        [Fact(DisplayName = "Clients must be able to override property values that have been configured for Property Injection with the DI container.")]
        public void OverrideKnownProperty()
        {
            Container.RegisterTransient<G>(options => options.AddPropertyInjection(p => p.ReferenceToA))
                     .RegisterTransient<A>();
            var instanceOfA = new A();

            var parameterOverrides = Container.OverrideParametersFor<G>().OverrideMember(nameof(G.ReferenceToA), instanceOfA);
            var instanceOfG = Container.Resolve<G>(parameterOverrides);

            instanceOfG.ReferenceToA.Should().BeSameAs(instanceOfA);
        }

        [Fact(DisplayName = "Clients must be able to override property values where Property Injection was not configured before with the DI Container.")]
        public void OverrideUnknownProperty()
        {
            Container.RegisterTransient<G>();
            var instanceOfA = new A();

            var parameterOverrides = Container.OverrideParametersFor<G>().OverrideMember(nameof(G.ReferenceToA), instanceOfA);
            var instanceOfG = Container.Resolve<G>(parameterOverrides);

            instanceOfG.ReferenceToA.Should().BeSameAs(instanceOfA);
        }

        [Fact(DisplayName = "Clients must be able to override field values that have been configured for Field Injection with the DI container.")]
        public void OverrideKnownField()
        {
            Container.RegisterTransient<J>(options => options.AddFieldInjection(o => o.ReferenceToG))
                     .RegisterTransient<G>();
            var instanceOfG = new G();

            var parameterOverrides = Container.OverrideParametersFor<J>().OverrideMember(nameof(J.ReferenceToG), instanceOfG);
            var instanceOfJ = Container.Resolve<J>(parameterOverrides);

            instanceOfJ.ReferenceToG.Should().BeSameAs(instanceOfG);
        }

        [Fact(DisplayName = "Clients must be able to override field values where Field Injection was not configured before with the DI Container.")]
        public void OverrideUnknownField()
        {
            var instanceOfG = new G();

            var parameterOverrides = Container.OverrideParametersFor<J>().OverrideMember(nameof(J.ReferenceToG), instanceOfG);
            var instanceOfJ = Container.Resolve<J>(parameterOverrides);

            instanceOfJ.ReferenceToG.Should().BeSameAs(instanceOfG);
        }

        [Fact(DisplayName = "Clients must be able to override instance injections using a field info.")]
        public void OverrideFieldInfo()
        {
            var instanceOfG = new G();

            var parameterOverrides = Container.OverrideParametersFor<J>().OverrideMember(typeof(J).GetField(nameof(J.ReferenceToG)), instanceOfG);
            var instanceOfJ = Container.Resolve<J>(parameterOverrides);

            instanceOfJ.ReferenceToG.Should().BeSameAs(instanceOfG);
        }

        [Fact(DisplayName = "Clients must be able to override instance injections using a property info.")]
        public void OverridePropertyInfo()
        {
            var instanceOfA = new A();

            var parameterOverrides = Container.OverrideParametersFor<G>().OverrideMember(typeof(G).GetProperty(nameof(G.ReferenceToA)), instanceOfA);
            var instanceOfG = Container.Resolve<G>(parameterOverrides);

            instanceOfG.ReferenceToA.Should().BeSameAs(instanceOfA);
        }
    }
}