using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public abstract class BaseOptionsTests : DefaultDiContainerTest
    {
        protected abstract void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions);

        [Fact(DisplayName = "Clients must be able to change the registration name using the options object.")]
        public void OptionsRegistrationName()
        {
            Register<A>(options => options.UseRegistrationName("Foo"));

            Container.Registrations.Should().ContainSingle(registration => registration.Name == "Foo");
        }

        [Fact(DisplayName = "Clients must be able to change the constructor that is used to instantiate the target object.")]
        public void OptionsSelectDefaultConstructor()
        {
            Register<D>(options => options.UseDefaultConstructor());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First());
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with a single parameter that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithOneParameter()
        {
            Register<D>(options => options.UseConstructorWithParameter<IList<int>>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(D).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 1));
        }

        [Fact(DisplayName = "Clients must be able to choose a constructor with two parameters that the DI container uses to instantiate the target type.")]
        public void OptionsConstructorWithTwoParameters()
        {
            Register<E>(options => options.UseConstructorWithParameters<int, uint>());

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == typeof(E).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2));
        }

        [Fact(DisplayName = "Clients must be able to register a type with mappings to all of its implemented interfaces.")]
        public void MapAllInterfaces()
        {
            Register<E>(options => options.MapToAllImplementedInterfaces()
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

        [Fact(DisplayName = "Clients must be able to register a type with mappings to specified base types.")]
        public void MapSpecificTypes()
        {
            Register<E>(options => options.MapToAbstractions(typeof(IE), typeof(A))
                                          .UseDefaultConstructor());

            var resolvedInstances = new object[]
                                    {
                                        Container.Resolve<IE>(),
                                        Container.Resolve<A>(),
                                        Container.Resolve<E>()
                                    };

            resolvedInstances.Should().ContainItemsAssignableTo<E>();
        }

        [Fact(DisplayName = "Clients must be able to pass a ConstructorInfo directly to the options that the DI container will use to instantiate the target type.")]
        public void OptionsPassingConstructorInfo()
        {
            var targetConstructor = typeof(E).GetTypeInfo().DeclaredConstructors.ElementAt(2);

            Register<E>(options => options.UseConstructor(targetConstructor));

            Container.Registrations.Should().ContainSingle(registration => ((ConstructorInstantiationInfo) registration.TypeCreationInfo.InstantiationInfo).ConstructorInfo == targetConstructor);
        }

        [Theory(DisplayName = "Clients must be able to register a static factory method instead of a constructor that the DI container uses to instantiate the target type.")]
        [MemberData(nameof(ResolveWithStaticFactoryMethodData))]
        public void ResolveWithStaticCreationMethod(Action<IRegistrationOptionsForType<F>> configureStaticMethod)
        {
            Register(configureStaticMethod);
            Container.RegisterInstance("Hello")
                     .RegisterInstance(3);

            var instanceOfF = Container.Resolve<F>();

            instanceOfF.Text.Should().Be(Container.Resolve<string>());
            instanceOfF.Number.Should().Be(Container.Resolve<int>());
        }

        public static readonly IEnumerable<object[]> ResolveWithStaticFactoryMethodData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.InstantiateVia(new Func<string, int, F>(F.Create))) },
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.UseStaticFactoryMethod(typeof(F).GetRuntimeMethod("Create", new[] { typeof(string), typeof(int) }))) }
            };

        [Theory(DisplayName = "Clients must be able to configure property injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(PropertyInjectionData))]
        public void PropertyInjection(Action<IRegistrationOptionsForType<G>> configurePropertyInjection)
        {
            Register(configurePropertyInjection);

            var instanceOfG = Container.Resolve<G>();

            instanceOfG.ReferenceToA.Should().NotBeNull();
        }

        public static readonly IEnumerable<object[]> PropertyInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<G>>(options => options.AddPropertyInjection(g => g.ReferenceToA)) },
                new object[] { new Action<IRegistrationOptionsForType<G>>(options => options.AddPropertyInjection(typeof(G).GetRuntimeProperty("ReferenceToA"))) }
            };

        [Theory(DisplayName = "Clients must be able to configure field injections that the DI container performs after an instance of the target type was created.")]
        [MemberData(nameof(FieldInjectionData))]
        public void FieldInjection(Action<IRegistrationOptionsForType<H>> configureFieldInjection)
        {
            Register(configureFieldInjection);
            Container.RegisterInstance(true);

            var instanceOfH = Container.Resolve<H>();

            instanceOfH.BooleanValue.Should().BeTrue();
        }

        public static readonly IEnumerable<object[]> FieldInjectionData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<H>>(options => options.AddFieldInjection(h => h.BooleanValue)) },
                new object[] { new Action<IRegistrationOptionsForType<H>>(options => options.AddFieldInjection(typeof(H).GetRuntimeField("BooleanValue"))) }
            };

        [Fact(DisplayName = "Clients must be able to add a registration name for property injections that the container uses to resolve the child value.")]
        public void ResolvePropertyInjectionWithNonDefaultRegistration()
        {
            const string registrationName = "MyAObject";
            Container.RegisterSingleton<A>(options => options.UseRegistrationName(registrationName));
            Register<G>(options => options.AddPropertyInjection(g => g.ReferenceToA, registrationName));

            var instanceOfG = Container.Resolve<G>();

            instanceOfG.ReferenceToA.Should().BeSameAs(Container.Resolve<A>(registrationName));
        }

        [Fact(DisplayName = "Clients must be able to override existing registrations.")]
        public void OverrideExistingMapping()
        {
            Register<E>(options => options.MapToAbstractions(typeof(IF)));
            Register<F>(options => options.MapToAbstractions(typeof(IF))
                                          .InstantiateVia<string, int>(F.Create));
            Container.RegisterInstance("Foo")
                     .RegisterInstance(42);

            var instance = Container.Resolve<IF>();

            instance.Should().BeAssignableTo<F>();
        }

        [Theory(DisplayName = "Clients must be able to add registration names for instantiation method parameters the container uses to resolve child values.")]
        [MemberData(nameof(ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData))]
        public void ResolveInstantiationMethodDependencyWithNonDefaultRegistration(Action<IRegistrationOptionsForType<B>> configureOptionsForB)
        {
            Container.RegisterTransient<A>(options => options.UseRegistrationName("MySpecialA"))
                     .RegisterInstance(42);
            Register(configureOptionsForB);

            var instanceOfB = Container.Resolve<B>();

            instanceOfB.OtherObject.Should().NotBeNull();
        }

        public static readonly IEnumerable<object[]> ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter<A>().WithName("MySpecialA")) },
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter("otherObject").WithName("MySpecialA")) }
            };

        [Fact(DisplayName = "Clients must be able to add a registration name for field injections that the container uses to resolve the child value.")]
        public void ResolveFieldInjectionWithNonDefaultRegistration()
        {
            Register<G>(options => options.UseTypeNameAsRegistrationName()
                                          .AddPropertyInjection(g => g.ReferenceToA));
            Register<J>(options => options.AddFieldInjection(j => j.ReferenceToG, nameof(G)));

            var instanceOfJ = Container.Resolve<J>();

            instanceOfJ.ReferenceToG.ReferenceToA.Should().NotBeNull();
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