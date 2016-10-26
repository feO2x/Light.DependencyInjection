using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Light.DependencyInjection.Lifetimes;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class AutomaticResolveTests : DefaultDependencyInjectionContainerTest
    {
        [Fact(DisplayName = "The DI container must create a transient registration and use it  when Resolve is called for a non-registered concrete type.")]
        public void ResolveDefaultTransient()
        {
            Container.Resolve<A>();

            Container.Registrations.Should().ContainSingle(registration => registration.TargetType == typeof(A) && registration.Lifetime is TransientLifetime);
        }

        [Fact(DisplayName = "The DI container must be able to create an instance of object without registering it.")]
        public void ResolveObject()
        {
            var instance = Container.Resolve<object>();
            instance.Should().NotBeNull();
        }

        [Theory(DisplayName = "The DI container is not able to resolve unregistered primitive types.")]
        [InlineData(typeof(int))]
        [InlineData(typeof(short))]
        [InlineData(typeof(long))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        public void ResolvePrimitiveError(Type primitiveType)
        {
            Action act = () => Container.Resolve(primitiveType);

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{primitiveType}\" is a primitive type which cannot be automatically resolved by the Dependency Injection Container.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an interface type that was not registered before.")]
        public void ResolveInterfaceError()
        {
            Action act = () => Container.Resolve<IC>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified interface type \"{typeof(IC)}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called on an abstract class type that was not registered before.")]
        public void ResolveAbstractClassError()
        {
            Action act = () => Container.Resolve<DefaultDependencyInjectionContainerTest>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified abstract class \"{typeof(DefaultDependencyInjectionContainerTest)}\" could not be resolved because there is no concrete type registered for it. Automatic resolve is not possible with types that cannot be instantiated.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called with a generic parameter type.")]
        public void ResolveGenericParameterTypeError()
        {
            var genericParameterType = typeof(List<>).GetTypeInfo().GenericTypeParameters.First();

            Action act = () => Container.Resolve(genericParameterType);

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{genericParameterType}\" is a generic parameter which cannot be resolved by the Dependency Injection Container.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called with a generic type definition.")]
        public void ResolveGenericTypeDefinitionError()
        {
            var genericTypeDefinition = typeof(List<>);

            Action act = () => Container.Resolve(genericTypeDefinition);

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{genericTypeDefinition}\" is a generic type definition which cannot be resolved by the Dependency Injection Container.");
        }

        [Fact(DisplayName = "The Di container must throw an exception when Resolve is called with an open generic type.")]
        public void ResolveOpenGenericTypeError()
        {
            var genericTypeDefinition = typeof(Dictionary<,>).GetTypeInfo();
            var openGenericType = genericTypeDefinition.MakeGenericType(typeof(string), genericTypeDefinition.GenericTypeParameters[1]);

            Action act = () => Container.Resolve(openGenericType);

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{openGenericType}\" is an open generic type which cannot be resolved by the Dependency Injection Container.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called with an enum type which was not registered before.")]
        public void ResolveUnregisteredEnumValue()
        {
            Action act = () => Container.Resolve<ConsoleColor>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(ConsoleColor)}\" describes an enum type which has not been registered and which cannot be resolved automatically.");
        }

        [Fact(DisplayName = "The DI container must throw an exception when Resolve is called with a delegate type.")]
        public void ResolveDelegateError()
        {
            Action act = () => Container.Resolve<Action>();

            act.ShouldThrow<ResolveTypeException>()
               .And.Message.Should().Contain($"The specified type \"{typeof(Action)}\" describes a delegate type which has not been registered and which cannot be resolved automatically.");
        }
    }
}