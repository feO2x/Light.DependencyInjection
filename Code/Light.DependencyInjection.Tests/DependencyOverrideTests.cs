using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Light.GuardClauses;
using Xunit;
// ReSharper disable ClassNeverInstantiated.Global

namespace Light.DependencyInjection.Tests
{
    [Trait("Category", "Functional Tests")]
    public sealed class DependencyOverrideTests
    {
        [Fact(DisplayName = "The client must be able to override an instantiation dependency by type.")]
        public void OverrideInstantitationDependencyByType()
        {
            var container = new DiContainer().Register<A>()
                                             .Register<B>()
                                             .Register<C>();

            var dependencyOverrides = container.OverrideDependenciesFor<C>();
            var myInstance = new B();
            dependencyOverrides.OverrideDependency(myInstance);
            var instanceOfC = container.Resolve<C>(dependencyOverrides);

            instanceOfC.B.Should().BeSameAs(myInstance);
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

        [Fact(DisplayName = "The client must be able to override an instantiation dependency by type.")]
        public void OverrideInstantitationDependencyByName()
        {
            var container = new DiContainer().Register("Foo", options => options.UseRegistrationName("The First String"))
                                             .Register("Bar", options => options.UseRegistrationName("The Second String"))
                                             .Register<D>(options => options.ConfigureInstantiationParameter("first", paramter => paramter.WithTargetRegistrationName("The First String"))
                                                                            .ConfigureInstantiationParameter("second", parameter => parameter.WithTargetRegistrationName("The Second String")));

            const string overriddenString = "Baz";
            var dependencyOverrides = container.OverrideDependenciesFor<D>()
                                               .OverrideDependency("second", overriddenString);

            var resolvedInstance = container.Resolve<D>(dependencyOverrides);

            resolvedInstance.First.Should().Be("Foo");
            resolvedInstance.Second.Should().Be(overriddenString);
        }

        public class D
        {
            public readonly string First;
            public readonly string Second;

            public D(string first, string second)
            {
                First = first;
                Second = second;
            }
        }

        [Fact(DisplayName = "The client must be able to override all instances of a certain type during a resolve call.")]
        public void OverrideInstanceByType()
        {
            var container = new DiContainer().Register<ClassWithoutDependencies>(options => options.UseSingletonLifetime())
                                             .Register<ClassWithDependency>()
                                             .Register<ClassWithTwoDependencies>();

            var overriddenInstance = new ClassWithoutDependencies();
            var dependencyOverrides = container.OverrideDependenciesFor<ClassWithTwoDependencies>()
                                               .OverrideRegistration(overriddenInstance);
            var resolvedInstance = container.Resolve<ClassWithTwoDependencies>(dependencyOverrides);

            resolvedInstance.A.Should().BeSameAs(overriddenInstance);
            resolvedInstance.B.A.Should().BeSameAs(overriddenInstance);
        }

        [Fact(DisplayName = "The DI Container must be able to resolve object graphs where dependencies are not registered but provided via DependencyOverrides.")]
        public void OverrideDependencyThatIsNotRegistered()
        {
            var containerServices = new ContainerServicesBuilder().WithAutomaticRegistrationFactory(new NoAutoRegistrationsAllowedFactory())
                                                                 .Build();
            var container = new DiContainer(containerServices).Register<ClassWithDependency>()
                                                              .Register<ClassWithTwoDependencies>();

            var overriddenInstance = new ClassWithoutDependencies();
            var dependencyOverrides = container.OverrideDependenciesFor<ClassWithTwoDependencies>()
                                               .OverrideDependency(overriddenInstance);
            var resolvedInstance = container.Resolve<ClassWithTwoDependencies>(dependencyOverrides);

            resolvedInstance.A.Should().BeSameAs(overriddenInstance);
            resolvedInstance.B.A.Should().BeSameAs(overriddenInstance);
        }

        [Fact(DisplayName = "The DI Container must be able to resolve object graphs where the overridden registration does not exist.")]
        public void OverrideNonExisitingRegistration()
        {
            var containerServices = new ContainerServicesBuilder().WithAutomaticRegistrationFactory(new NoAutoRegistrationsAllowedFactory())
                                                                  .Build();
            var container = new DiContainer(containerServices).Register<ClassWithDependency>()
                                                              .Register<ClassWithTwoDependencies>();

            var overriddenInstance = new ClassWithoutDependencies();
            var dependencyOverrides = container.OverrideDependenciesFor<ClassWithTwoDependencies>()
                                               .OverrideRegistration(overriddenInstance);
            var resolvedInstance = container.Resolve<ClassWithTwoDependencies>(dependencyOverrides);

            resolvedInstance.A.Should().BeSameAs(overriddenInstance);
            resolvedInstance.B.A.Should().BeSameAs(overriddenInstance);
        }
    }
}