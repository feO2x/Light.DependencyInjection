using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ErroneousTypeRegistrationTests : DefaultDiContainerTests
    {
        [Fact(DisplayName = "Open constructed generic types must not be registered with the DI container.")]
        public void OpenConstructedGenericTypesNotAllowed()
        {
            var openConstructedGenericType = typeof(SubType<>).GetTypeInfo().BaseType;

            Action act = () => Container.RegisterTransient(openConstructedGenericType);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The type \"{openConstructedGenericType}\" cannot be registered with the DI container because it is an open constructed generic type. Only non-generic types, closed constructed generic types or generic type definitions can be registered.");
        }

        // ReSharper disable UnusedTypeParameter
        public class BaseType<T1, T2> { }

        // ReSharper restore UnusedTypeParameter

        public class SubType<T> : BaseType<T, int> { }

        [Fact(DisplayName = "Interface types cannot be registered with the DI container because they cannot be instantiated.")]
        public void InterfaceTypesNotAllowed()
        {
            Action act = () => Container.RegisterTransient(typeof(IE));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The type \"{typeof(IE)}\" cannot be registered with the DI container because it is an interface type that cannot be instantiated.");
        }

        [Fact(DisplayName = "Abstract base classes cannot be registered with the DI container because they cannot be instantiated.")]
        public void AbstractBaseClassesNotAllowed()
        {
            Action act = () => Container.RegisterTransient(typeof(DefaultDiContainerTests));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The type \"{typeof(DefaultDiContainerTests)}\" cannot be registered with the DI container because it is an abstract class that cannot be instantiated.");
        }

        [Fact(DisplayName = "Generic parameter types cannot be registered with the DI container because they cannot be instantiated.")]
        public void GenericParameterTypeNotAllowed()
        {
            var genericParameterType = typeof(Dictionary<,>).GetTypeInfo().GenericTypeParameters.First();

            Action act = () => Container.RegisterTransient(genericParameterType);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The type \"{genericParameterType}\" cannot be registered with the DI container because it is a generic type paramter. Only non-generic types, closed constructed generic types or generic type definitions can be registered.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when a type is registered that has no public constructor and where no instantiation method is provided.")]
        public void NoPublicConstructor()
        {
            var typeWithoutPublicConstructor = typeof(Foo);

            Action act = () => Container.RegisterTransient(typeWithoutPublicConstructor);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"Cannot register \"{typeWithoutPublicConstructor}\" with the DI container because this type does not contain a public non-static constructor. Please specify an instantiation method using the registration options.");
        }

        public class Foo
        {
            private Foo() { }
        }
    }
}