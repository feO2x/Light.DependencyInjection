using FluentAssertions;
using Light.GuardClauses;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class DependencyOverrideTests
    {
        [Fact(DisplayName = "Individual instatiation parameters must be resolved ")]
        public void OverrideInstantitationDependency()
        {
            var container = new DiContainer().Register<A>()
                                             .Register<B>()
                                             .Register<C>();

            var dependencyOverrides = container.OverrideDependenciesFor<C>();
            var overriddenInstance = new B();
            dependencyOverrides.Override(overriddenInstance);
            var instanceOfC = container.Resolve<C>(dependencyOverrides);

            instanceOfC.B.Should().BeSameAs(overriddenInstance);
        }

        public class A { }

        public class B { }

        public class C
        {
            public readonly A A;
            public readonly B B;

            public C(A a, B b)
            {
                A = a.MustNotBeNull();
                B = b.MustNotBeNull();
            }
        }
    }
}