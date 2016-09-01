using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests
{
    public sealed class ResolveTests : DefaultDiContainerTests
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

            Container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration is TransientRegistration);
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an abstract type that was not registered before.")]
        public void ResolveAbstractionError()
        {
            Action act = () => Container.Resolve<IC>();

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(IC)}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.");
        }

        [Fact(DisplayName = "Clients must be able to change the registration name using the options object when calling ResolveTransient.")]
        public void OptionsRegistrationName()
        {
            Container.RegisterTransient<A>(options => options.UseRegistrationName("Foo"));

            Container.Registrations.Should().ContainSingle(registration => registration.Name == "Foo");
        }

        [Fact(DisplayName = "Clients must be able to change the constructor that is used to instantiate the target object.")]
        public void OptionsSelectDefaultConstructor()
        {
            Container.RegisterTransient<D>(options => options.UseDefaultConstructor());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First());
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with a single parameter that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithOneParameter()
        {
            Container.RegisterTransient<D>(options => options.UseConstructorWithParameter<IList<int>>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 1));
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with two parameters that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithTwoParameters()
        {
            Container.RegisterTransient<E>(options => options.UseConstructorWithParameters<int, uint>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(E).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2));
        }

        [Fact(DisplayName = "Clients must be able to pass a ConstructorInfo directly to the options that the DI container will use to instantiate the target type.")]
        public void OptionsPassingConstructorInfo()
        {
            var targetConstructor = typeof(E).GetTypeInfo().DeclaredConstructors.ElementAt(2);

            Container.RegisterTransient<E>(options => options.UseConstructor(targetConstructor));

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == targetConstructor);
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to all of its implemented interfaces.")]
        public void MapAllInterfaces()
        {
            Container.RegisterTransient<E>(options => options.MapTypeToAllImplementedInterfaces()
                                                              .UseDefaultConstructor());

            var resolvedInstances = new object[]
                                    {
                                        Container.Resolve<IE>(),
                                        Container.Resolve<IF>(),
                                        Container.Resolve<IG>(),
                                        Container.Resolve<E>()
                                    };

            resolvedInstances.Should().ContainItemsAssignableTo<E>();
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to specified base types")]
        public void MapSpecificTypes()
        {
            Container.RegisterTransient<E>(options => options.MapTypeToAbstractions(typeof(IE), typeof(A))
                                                              .UseDefaultConstructor());

            var resolvedInstances = new object[]
                                    {
                                        Container.Resolve<IE>(),
                                        Container.Resolve<A>(),
                                        Container.Resolve<E>()
                                    };

            resolvedInstances.Should().ContainItemsAssignableTo<E>();
        }

        [Theory(DisplayName = "Clients must be able to register a static factory method instead of a constructor that the DI container uses to instantiate the target type.")]
        [MemberData(nameof(ResolveWithStaticFactoryMethodData))]
        public void ResolveWithStaticFactoryMethod(Action<IRegistrationOptions<F>> configureStaticMethod)
        {
            Container.RegisterTransient(configureStaticMethod)
                      .RegisterInstance("Hello")
                      .RegisterInstance(3);

            var instanceOfF = Container.Resolve<F>();

            instanceOfF.Text.Should().Be(Container.Resolve<string>());
            instanceOfF.Number.Should().Be(Container.Resolve<int>());
        }

        public static readonly TestData ResolveWithStaticFactoryMethodData =
            new[]
            {
                new object[] { new Action<IRegistrationOptions<F>>(options => options.UseStaticFactoryMethod(new Func<string, int, F>(F.Create))) },
                new object[] { new Action<IRegistrationOptions<F>>(options => options.UseStaticFactoryMethod(() => F.Create(default(string), default(int)))) },
                new object[] { new Action<IRegistrationOptions<F>>(options => options.UseStaticFactoryMethod(typeof(F).GetRuntimeMethod("Create", new[] { typeof(string), typeof(int) }))) }
            };

        [Theory(DisplayName = "Clients must be able to configure property injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(PropertyInjectionData))]
        public void PropertyInjection(Action<IRegistrationOptions<G>> configurePropertyInjection)
        {
            Container.RegisterTransient(configurePropertyInjection);

            var instanceOfG = Container.Resolve<G>();

            instanceOfG.ReferenceToA.Should().NotBeNull();
        }

        public static readonly TestData PropertyInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptions<G>>(options => options.AddPropertyInjection(g => g.ReferenceToA)) },
                new object[] { new Action<IRegistrationOptions<G>>(options => options.AddPropertyInjection(typeof(G).GetRuntimeProperty("ReferenceToA"))) }
            };

        [Theory(DisplayName = "Clients must be able to configure field injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(FieldInjectionData))]
        public void FieldInjection(Action<IRegistrationOptions<H>> configureFieldInjection)
        {
            Container.RegisterTransient(configureFieldInjection)
                      .RegisterInstance(true);

            var instanceOfH = Container.Resolve<H>();

            instanceOfH.BooleanValue.Should().BeTrue();
        }

        public static readonly TestData FieldInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptions<H>>(options => options.AddFieldInjection(h => h.BooleanValue)) },
                new object[] { new Action<IRegistrationOptions<H>>(options => options.AddFieldInjection(typeof(H).GetRuntimeField("BooleanValue"))) }
            };

        [Fact(DisplayName = "Clients must be able to add a registration name for property injections that the container uses to resolve the child value.")]
        public void ResolvePropertyInjectionWithNonDefaultRegistration()
        {
            Container.RegisterTransient<A>("MyAObject")
                      .RegisterTransient<G>(options => options.AddPropertyInjection(g => g.ReferenceToA, "MyAObject"));

            var instanceOfG = Container.Resolve<G>();

            instanceOfG.ReferenceToA.Should().NotBeNull();
        }

        [Fact(DisplayName = "Clients must be able to add a registration name for field injections that the container uses to resolve the child value.")]
        public void ResolveFieldInjectionWithNonDefaultRegistration()
        {
            Container.RegisterTransient<G>()
                      .RegisterTransient<G>(options => options.UseRegistrationName("MyG")
                                                              .AddPropertyInjection(g => g.ReferenceToA))
                      .RegisterTransient<J>(options => options.AddFieldInjection(j => j.ReferenceToG, "MyG"));

            var instanceOfJ = Container.Resolve<J>();

            instanceOfJ.ReferenceToG.ReferenceToA.Should().NotBeNull();
        }

        [Theory(DisplayName = "Clients must be able to add registration names for instantiation method parameters the container uses to resolve child values.")]
        [MemberData(nameof(ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData))]
        public void ResolveInstantiationMethodDependencyWithNonDefaultRegistration(Action<IRegistrationOptions<B>> configureOptionsForB)
        {
            Container.RegisterTransient<A>("MySpecialA")
                      .RegisterTransient(configureOptionsForB)
                      .RegisterInstance(42);

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.OtherObject.Should().NotBeNull();
        }

        public static readonly TestData ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData =
            new[]
            {
                new object[] { new Action<IRegistrationOptions<B>>(options => options.ResolveInstantiationParameter<A>().WithName("MySpecialA")) },
                new object[] { new Action<IRegistrationOptions<B>>(options => options.ResolveInstantiationParameter("otherObject").WithName("MySpecialA")) }
            };

        [Fact(DisplayName = "Clients must be able to override parameter values when resolving a type.")]
        public void OverrideInstantiationParameters()
        {
            Container.RegisterTransient<B>()
                     .RegisterTransient<A>()
                     .RegisterInstance(42);

            var parameterOverrides = Container.OverrideParametersFor<B>().OverrideInstantiationParameter("value", 87);
            var instanceOfB = Container.Resolve<B>(parameterOverrides);

            instanceOfB.Value.Should().Be(87);
        }
    }
}