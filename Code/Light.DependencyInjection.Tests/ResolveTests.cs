using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests : DefaultDependencyInjectionContainerTest
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

        [Fact(DisplayName = "The DI Container must be able to inject itself when it's type without registration name is resolved.")]
        public void SelfInject()
        {
            var instance = Container.Resolve<ServiceLocatorClient>();

            instance.Container.Should().BeSameAs(Container);
        }

        [Fact(DisplayName = "The DI container must resolve itself when an IServiceProvider instance is requested.")]
        public void SelfInjectServiceProvider()
        {
            var instance = Container.Resolve<IServiceProvider>();

            instance.Should().BeSameAs(Container);
        }

        [Fact(DisplayName = "The DI container must be able to resolve types that are instantiated via lambdas.")]
        public void InstantiateViaLambda()
        {
            Container.RegisterTransient<D>(options => options.InstantiateWith((IList<int> collection) => new D(collection, 90)))
                     .RegisterTransient(typeof(List<>), options => options.UseDefaultConstructor()
                                                                          .MapToAbstractions(typeof(IList<>)));

            var instanceOfD = Container.Resolve<D>();

            instanceOfD.SomeNumber.Should().Be(90);
            instanceOfD.Collection.Should().NotBeNull();
        }

        [Fact(DisplayName = "The DI container must be able to resolve types that are instantiated via instance methods.")]
        public void InstantiateViaInstanceMethod()
        {
            Container.RegisterTransient<B>(options => options.InstantiateWith<A>(CreateB));

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.ReferenceToA.Should().NotBeNull();
            instanceOfB.Value.Should().Be(_intValue);
        }

        private int _intValue;

        private B CreateB(A instanceOfA)
        {
            _intValue = new Random().Next();
            return new B(instanceOfA, _intValue);
        }

        [Fact(DisplayName = "Clients must be able to register delegate instantiation methods when using the non-generic registration options.")]
        public void UseDelegateWithNonGenericOptions()
        {
            Container.RegisterTransient(typeof(B), options => options.InstantiateWith((A instanceOfA) => new B(instanceOfA, 67)));

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.ReferenceToA.Should().NotBeNull();
            instanceOfB.Value.Should().Be(67);
        }
    }
}