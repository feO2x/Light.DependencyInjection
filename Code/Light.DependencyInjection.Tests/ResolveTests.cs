using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests
    {
        private readonly DiContainer _container = new DiContainer();

        [Fact(DisplayName = "The DI container must return one and the same instance when a type is registered as a Singleton.")]
        public void ResolveSingleton()
        {
            _container.RegisterSingleton<A>();

            var first = _container.Resolve<A>();
            var second = _container.Resolve<A>();

            first.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must return new instances when a type is registered as with a transient lifetime.")]
        public void ResolveTransient()
        {
            _container.RegisterTransient<A>();

            var first = _container.Resolve<A>();
            var second = _container.Resolve<A>();

            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a type depending on other objects / values. These values must be resolved recursively.")]
        public void ResolveRecursively()
        {
            _container.RegisterSingleton<A>()
                      .RegisterTransient<C>();

            var firstC = _container.Resolve<C>();
            var secondC = _container.Resolve<C>();

            firstC.Should().NotBeSameAs(secondC);
            firstC.ReferenceToA.Should().BeSameAs(secondC.ReferenceToA);
        }

        [Fact(DisplayName = "The DI container must be able to resolve a concrete type for a polymorphic abstraction when this mapping was registered with it beforehand.")]
        public void AbstractionMapping()
        {
            _container.RegisterSingleton<A>()
                      .RegisterTransient<IC, C>();

            var interfaceReference = _container.Resolve<IC>();

            interfaceReference.Should().BeOfType<C>();
        }

        [Fact]
        public void RegisterInstance()
        {
            var instance = new A();
            _container.RegisterInstance(instance);

            var resolvedInstance = _container.Resolve<A>();

            resolvedInstance.Should().BeSameAs(instance);
        }

        [Fact(DisplayName = "The DI container must create a transient registration and use it  when Resolve is called for a non-registered type.")]
        public void ResolveDefaultTransient()
        {
            _container.Resolve<A>();

            _container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration is TransientRegistration);
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an abstract type that was not registered before.")]
        public void ResolveAbstractionError()
        {
            Action act = () => _container.Resolve<IC>();

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(IC)}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.");
        }

        [Fact(DisplayName = "Clients must be able to change the registration name using the options object when calling ResolveTransient.")]
        public void OptionsRegistrationName()
        {
            _container.RegisterTransient<A>(options => options.WithRegistrationName("Foo"));

            _container.Registrations.Should().ContainSingle(registration => registration.Name == "Foo");
        }

        [Fact(DisplayName = "Clients must be able to change the constructor that is used to instantiate the target object.")]
        public void OptionsSelectDefaultConstructor()
        {
            _container.RegisterTransient<D>(options => options.UseDefaultConstructor());

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.TargetCreationMethodInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First());
        }

        [Fact(DisplayName = "UseDefaultConstructor must throw a TypeRegistrationException when the target type has no default constructor.")]
        public void OptionsErroneousDefaultConstructor()
        {
            Action act = () => _container.RegisterTransient<B>(options => options.UseDefaultConstructor());

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You specified that the DI container should use the default constructor of type \"{typeof(B)}\", but this type contains no default constructor.");
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with a single parameter that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithOneParameter()
        {
            _container.RegisterTransient<D>(options => options.UseConstructorWithParameter<IList<int>>());

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.TargetCreationMethodInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 1));
        }

        [Fact(DisplayName = "UseConstructorWithParameter must throw a TypeRegistrationException when the target type does not contain the specified target constructor.")]
        public void OptionsErroneousConstructorWithOneParameter()
        {
            Action act = () => _container.RegisterTransient<B>(options => options.UseConstructorWithParameter<A>());

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You specified that the DI container should use the constructor with a single argument of type \"{typeof(A)}\", but type \"{typeof(B)}\" does not contain such a constructor.");
        }
    }
}