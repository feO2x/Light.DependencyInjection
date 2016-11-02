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
    public abstract class BaseOptionsTests : DefaultDependencyInjectionContainerTest
    {
        protected abstract void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions);
        protected abstract void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null);

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
                new object[] { new Action<IRegistrationOptionsForType<F>>(options => options.InstantiateWith(new Func<string, int, F>(F.Create))) },
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
                                          .InstantiateWith<string, int>(F.Create));
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

            instanceOfB.ReferenceToA.Should().NotBeNull();
        }

        public static readonly IEnumerable<object[]> ResolveInstantiationMethodDependencyWithNonDefaultRegistrationData =
            new[]
            {
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter<A>().WithName("MySpecialA")) },
                new object[] { new Action<IRegistrationOptionsForType<B>>(options => options.ResolveInstantiationParameter("referenceToA").WithName("MySpecialA")) }
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

        [Fact(DisplayName = "Clients must be able to add a property injection with a property from a target type's base class.")]
        public void PropertyInjectionWithPropertyFromBaseClass()
        {
            Register<SubG>(options => options.AddPropertyInjection(g => g.ReferenceToA));

            var subG = Container.Resolve<SubG>();

            subG.ReferenceToA.Should().NotBeNull();
        }

        [Fact(DisplayName = "Clients must be able to add a field injection with a field from a target type's base class.")]
        public void FieldInjectionWithFieldFromBaseClass()
        {
            Register<SubJ>(options => options.AddFieldInjection(j => j.ReferenceToG));

            var subJ = Container.Resolve<SubJ>();

            subJ.ReferenceToG.Should().NotBeNull();
        }

        [Fact(DisplayName = "Open constructed generic types must not be registered with the DI container.")]
        public void OpenConstructedGenericTypesNotAllowed()
        {
            var openConstructedGenericType = typeof(SubType<>).GetTypeInfo().BaseType;

            Action act = () => Register(openConstructedGenericType);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type \"{openConstructedGenericType}\" because it is a bound open generic type. Please ensure that you provide the generic type definition of this type.");
        }

        // ReSharper disable UnusedTypeParameter
        public class BaseType<T1, T2> { }

        // ReSharper restore UnusedTypeParameter

        public class SubType<T> : BaseType<T, int> { }

        [Fact(DisplayName = "Interface types cannot be registered with the DI container because they cannot be instantiated.")]
        public void InterfaceTypesNotAllowed()
        {
            Action act = () => Register(typeof(IE));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type \"{typeof(IE)}\" because it is an interface which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact(DisplayName = "Abstract base classes cannot be registered with the DI container because they cannot be instantiated.")]
        public void AbstractBaseClassesNotAllowed()
        {
            Action act = () => Register(typeof(DefaultDependencyInjectionContainerTest));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type \"{typeof(DefaultDependencyInjectionContainerTest)}\" because it is an abstract class which cannot be instantiated. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact(DisplayName = "Generic parameter types cannot be registered with the DI container because they cannot be instantiated.")]
        public void GenericParameterTypeNotAllowed()
        {
            var genericParameterType = typeof(Dictionary<,>).GetTypeInfo().GenericTypeParameters.First();

            Action act = () => Register(genericParameterType);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You cannot register type \"{genericParameterType}\" because it is a generic parameter type. Only non-abstract types that are either non-generic, closed generic, or generic type definitions are allowed.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when a type is registered that has no public constructor and where no instantiation method is provided.")]
        public void NoPublicConstructor()
        {
            var typeWithoutPublicConstructor = typeof(Foo);

            Action act = () => Register(typeWithoutPublicConstructor);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"Cannot register \"{typeWithoutPublicConstructor}\" with the DI container because this type does not contain a public non-static constructor. Please specify an instantiation method using the registration options.");
        }

        [Fact(DisplayName = "Clients must be able to choose non-public constructors to instantiate concrete types.")]
        public void ChooseDefaultNonPublicConstructor()
        {
            Container.RegisterTransient<Foo>(options => options.UseDefaultConstructor());

            var fooInstance = Container.Resolve<Foo>();

            fooInstance.Should().NotBeNull();
        }

        public class Foo
        {
            private Foo() { }
        }
    }

    public sealed class TransientOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterTransient(configureOptions);
        }

        protected override void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            Container.RegisterTransient(type, configureOptions);
        }
    }

    public sealed class SingletonOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterSingleton(configureOptions);
        }

        protected override void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            Container.RegisterSingleton(type, configureOptions);
        }
    }

    public sealed class ScopedOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterScoped(configureOptions);
        }

        protected override void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            Container.RegisterScoped(type, configureOptions);
        }
    }

    public sealed class PerThreadOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterPerThread(configureOptions);
        }

        protected override void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            Container.RegisterPerThread(type, configureOptions);
        }
    }

    public sealed class PerResolveOptionsTests : BaseOptionsTests
    {
        protected override void Register<T>(Action<IRegistrationOptionsForType<T>> configureOptions)
        {
            Container.RegisterPerResolve(configureOptions);
        }

        protected override void Register(Type type, Action<IRegistrationOptionsForType> configureOptions = null)
        {
            Container.RegisterPerResolve(type, configureOptions);
        }
    }
}