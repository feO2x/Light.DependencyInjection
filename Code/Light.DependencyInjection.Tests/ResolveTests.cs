using System;
using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.Lifetimes;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests : DefaultDiContainerTest
    {
        [Fact(DisplayName = "The DI container must return one and the same instance when a type is registered as a Singleton.")]
        public void ResolveSingleton()
        {
            Container.RegisterSingleton<A>();

            var first = Container.Resolve<A>();
            var second = Container.Resolve<A>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must return new instances when a type is registered as with a transient lifetime.")]
        public void ResolveTransient()
        {
            Container.RegisterTransient<A>();

            var first = Container.Resolve<A>();
            var second = Container.Resolve<A>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a type depending on other objects / values. These values must be resolved recursively.")]
        public void ResolveRecursively()
        {
            Container.RegisterSingleton<A>()
                     .RegisterTransient<C>();

            var firstC = Container.Resolve<C>();
            var secondC = Container.Resolve<C>();

            firstC.Should().NotBeSameAs(secondC);
            firstC.ReferenceToA.Should().BeSameAs(secondC.ReferenceToA);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a concrete type for a polymorphic abstraction when this mapping was registered with it beforehand.")]
        public void AbstractionMapping()
        {
            Container.RegisterSingleton<A>()
                     .RegisterTransient<IC, C>();

            var interfaceReference = Container.Resolve<IC>();

            interfaceReference.Should().BeOfType<C>();
        }

        [Fact(DisplayName = "The client must be able to register an instance that is handled as a singleton by the DI container.")]
        public void RegisterInstance()
        {
            var instance = new A();
            Container.RegisterInstance(instance);

            var resolvedInstance = Container.Resolve<A>();

            resolvedInstance.Should().BeSameAs(instance);
        }

        [Fact(DisplayName = "The DI container must create a transient registration and use it  when Resolve is called for a non-registered type.")]
        public void ResolveDefaultTransient()
        {
            Container.Resolve<A>();

            Container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration.Lifetime is TransientLifetime);
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an abstract type that was not registered before.")]
        public void ResolveAbstractionError()
        {
            Action act = () => Container.Resolve<IC>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(IC)}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.");
        }


        [Fact(DisplayName = "The DI Container must be able to inject itself when it's type without registration name is resolved.")]
        public void SelfInject()
        {
            var instance = Container.Resolve<ServiceLocator>();

            instance.Container.Should().BeSameAs(Container);
        }

        [Fact(DisplayName = "The DI container must be able to resolve types that are instantiated via lambdas.")]
        public void InstantiateViaLambda()
        {
            Container.RegisterTransient<D>(options => options.UseDelegate((IList<int> collection) => new D(collection, 90)))
                     .RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor()
                                                                          .MapToAbstractions(typeof(IList<>)));

            var instanceOfD = Container.Resolve<D>();

            instanceOfD.SomeNumber.Should().Be(90);
            instanceOfD.Collection.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI container must be able to resolve types that are instantiated via instance methods.")]
        public void InstantiateViaInstanceMethod()
        {
            Container.RegisterTransient<B>(options => options.UseDelegate<A>(CreateB));

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.OtherObject.Should().NotBeNull();
            instanceOfB.Value.Should().Be(_intValue);
        }

        private int _intValue;

        private B CreateB(A instanceOfA)
        {
            _intValue = new Random().Next();
            return new B(instanceOfA, _intValue);
        }
    }
}