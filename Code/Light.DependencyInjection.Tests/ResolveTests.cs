using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

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

        [Fact(DisplayName = "The client must be able to register an instance that is handled as a singleton by the DI container.")]
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

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.CreationMethodInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First());
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with a single parameter that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithOneParameter()
        {
            _container.RegisterTransient<D>(options => options.UseConstructorWithParameter<IList<int>>());

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.CreationMethodInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 1));
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with two parameters that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithTwoParameters()
        {
            _container.RegisterTransient<E>(options => options.UseConstructorWithParameters<int, uint>());

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.CreationMethodInfo == typeof(E).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2));
        }

        [Fact(DisplayName = "Clients must be able to pass a ConstructorInfo directly to the options that the DI container will use to instantiate the target type.")]
        public void OptionsPassingConstructorInfo()
        {
            var targetConstructor = typeof(E).GetTypeInfo().DeclaredConstructors.ElementAt(2);

            _container.RegisterTransient<E>(options => options.UseConstructor(targetConstructor));

            _container.Registrations.Should().ContainSingle(registration => registration.TypeInstantiationInfo.CreationMethodInfo == targetConstructor);
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to all of its implemented interfaces.")]
        public void MapAllInterfaces()
        {
            _container.RegisterTransient<E>(options => options.MapTypeToAllImplementedInterfaces()
                                                              .UseDefaultConstructor());

            var resolvedInstances = new object[]
                                    {
                                        _container.Resolve<IE>(),
                                        _container.Resolve<IF>(),
                                        _container.Resolve<IG>(),
                                        _container.Resolve<E>()
                                    };

            resolvedInstances.Should().ContainItemsAssignableTo<E>();
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to specified base types")]
        public void MapSpecificTypes()
        {
            _container.RegisterTransient<E>(options => options.MapTypeToAbstractions(typeof(IE), typeof(A))
                                                              .UseDefaultConstructor());

            var resolvedInstances = new object[]
                                    {
                                        _container.Resolve<IE>(),
                                        _container.Resolve<A>(),
                                        _container.Resolve<E>()
                                    };

            resolvedInstances.Should().ContainItemsAssignableTo<E>();
        }

        [Theory(DisplayName = "Clients must be able to register a static factory method instead of a constructor that the DI container uses to instantiate the target type.")]
        [MemberData(nameof(ResolveWithStaticFactoryMethodData))]
        public void ResolveWithStaticFactoryMethod(Action<IRegistrationOptions> configureStaticMethod)
        {
            _container.RegisterTransient<F>(configureStaticMethod)
                      .RegisterInstance("Hello")
                      .RegisterInstance(3);

            var instanceOfF = _container.Resolve<F>();

            instanceOfF.Text.Should().Be(_container.Resolve<string>());
            instanceOfF.Number.Should().Be(_container.Resolve<int>());
        }

        public static readonly TestData ResolveWithStaticFactoryMethodData =
            new[]
            {
                new object[] { new Action<IRegistrationOptions>(options => options.UseStaticFactoryMethod(new Func<string, int, F>(F.Create))) },
                new object[] { new Action<IRegistrationOptions>(options => options.UseStaticFactoryMethod(() => F.Create(default(string), default(int)))) },
                new object[] { new Action<IRegistrationOptions>(options => options.UseStaticFactoryMethod(typeof(F).GetRuntimeMethod("Create", new[] { typeof(string), typeof(int) }))) }
            };
    }
}