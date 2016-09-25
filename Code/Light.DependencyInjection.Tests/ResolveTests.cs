using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Lifetimes;
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

            Container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration.Lifetime is TransientLifetime);
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an abstract type that was not registered before.")]
        public void ResolveAbstractionError()
        {
            Action act = () => Container.Resolve<IC>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(IC)}\" could not be resolved because there is no concrete type registered that should be returned for this polymorphic abstraction.");
        }


        

        

        [Fact(DisplayName = "Clients must be able to choose a constructor with two parameters that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithTwoParameters()
        {
            Container.RegisterTransient<E>(options => options.UseConstructorWithParameters<int, uint>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(E).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2));
        }

        [Fact(DisplayName = "Clients must be able to pass a ConstructorInfo directly to the options that the DI container will use to instantiate the target type.")]
        public void OptionsPassingConstructorInfo()
        {
            var targetConstructor = typeof(E).GetTypeInfo().DeclaredConstructors.ElementAt(2);

            Container.RegisterTransient<E>(options => options.UseConstructor(targetConstructor));

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == targetConstructor);
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to all of its implemented interfaces.")]
        public void MapAllInterfaces()
        {
            Container.RegisterTransient<E>(options => options.MapToAllImplementedInterfaces()
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
            Container.RegisterTransient<E>(options => options.MapToAbstractions(typeof(IE), typeof(A))
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
        public void ResolveWithStaticFactoryMethod(Action<IRegistrationOptionsForType<F>> configureStaticMethod)
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
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.UseStaticFactoryMethod(new Func<string, int, F>(F.Create))) },
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.UseStaticFactoryMethod(() => F.Create(default(string), default(int)))) },
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.UseStaticFactoryMethod(typeof(F).GetRuntimeMethod("Create", new[] { typeof(string), typeof(int) }))) }
            };

        [Theory(DisplayName = "Clients must be able to configure property injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(PropertyInjectionData))]
        public void PropertyInjection(Action<IRegistrationOptionsForType<G>> configurePropertyInjection)
        {
            Container.RegisterTransient(configurePropertyInjection);

            var instanceOfG = Container.Resolve<G>();

            instanceOfG.ReferenceToA.Should().NotBeNull();
        }

        public static readonly TestData PropertyInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<G>>(options => options.AddPropertyInjection(g => g.ReferenceToA)) },
                new object[] { new Action<IRegistrationOptionsForType<G>>(options => options.AddPropertyInjection(typeof(G).GetRuntimeProperty("ReferenceToA"))) }
            };

        [Theory(DisplayName = "Clients must be able to configure field injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(FieldInjectionData))]
        public void FieldInjection(Action<IRegistrationOptionsForType<H>> configureFieldInjection)
        {
            Container.RegisterTransient(configureFieldInjection)
                     .RegisterInstance(true);

            var instanceOfH = Container.Resolve<H>();

            instanceOfH.BooleanValue.Should().BeTrue();
        }

        public static readonly TestData FieldInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<H>>(options => options.AddFieldInjection(h => h.BooleanValue)) },
                new object[] { new Action<IRegistrationOptionsForType<H>>(options => options.AddFieldInjection(typeof(H).GetRuntimeField("BooleanValue"))) }
            };

        [Fact(DisplayName = "Clients must be able to add a registration name for property injections that the container uses to resolve the child value.")]
        public void ResolvePropertyInjectionWithNonDefaultRegistration()
        {
            Container.RegisterTransient<A>(options => options.UseRegistrationName("MyAObject"))
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
        public void ResolveInstantiationMethodDependencyWithNonDefaultRegistration(Action<IRegistrationOptionsForType<B>> configureOptionsForB)
        {
            Container.RegisterTransient<A>(options => options.UseRegistrationName("MySpecialA"))
                     .RegisterTransient(configureOptionsForB)
                     .RegisterInstance(42);

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.OtherObject.Should().NotBeNull();
        }

        public static readonly TestData ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter<A>().WithName("MySpecialA")) },
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter("otherObject").WithName("MySpecialA")) }
            };

        [Fact(DisplayName = "The DI Container must be able to inject itself when it's type without registration name is resolved.")]
        public void SelfInject()
        {
            var instance = Container.Resolve<ServiceLocator>();

            instance.Container.Should().BeSameAs(Container);
        }

        [Fact(DisplayName = "Clients must be able to override existing registrations.")]
        public void OverrideExistingMapping()
        {
            Container.RegisterTransient<E>(options => options.MapToAbstractions(typeof(IF)))
                     .RegisterTransient<F>(options => options.MapToAbstractions(typeof(IF))
                                                             .UseStaticFactoryMethod(() => F.Create(default(string), default(int))))
                     .RegisterInstance("Foo")
                     .RegisterInstance(42);

            var instance = Container.Resolve<IF>();

            instance.Should().BeAssignableTo<F>();
        }
    }

    public abstract class BaseOptionsTests : DefaultDiContainerTests
    {
        protected abstract void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions);

        [Fact(DisplayName = "Clients must be able to change the registration name using the options object when calling ResolveTransient.")]
        public void OptionsRegistrationName()
        {
            Register<A>(options => options.UseRegistrationName("Foo"));

            Container.Registrations.Should().ContainSingle(registration => registration.Name == "Foo");
        }

        [Fact(DisplayName = "Clients must be able to change the constructor that is used to instantiate the target object.")]
        public void OptionsSelectDefaultConstructor()
        {
            Register<D>(options => options.UseDefaultConstructor());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First());
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with a single parameter that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithOneParameter()
        {
            Register<D>(options => options.UseConstructorWithParameter<IList<int>>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo)registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 1));
        }
    }

    public sealed class TransientOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterTransient(configureOptions);
        }
    }

    public sealed class SingletonOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterSingleton(configureOptions);
        }
    }

    public sealed class ScopedOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterScoped(configureOptions);
        }
    }

    public sealed class PerThreadOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterPerThread(configureOptions);
        }
    }

    public sealed class PerResolveOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterPerResolve(configureOptions);
        }
    }
}