using System;
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
    }
}