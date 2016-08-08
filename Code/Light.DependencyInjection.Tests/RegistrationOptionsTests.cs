using System;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Light.DependencyInjection.FrameworkExtensions;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.TypeConstruction;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests
{
    public sealed class RegistrationOptionsTests
    {
        [Theory(DisplayName = "UseDefaultConstructor must throw a TypeRegistrationException when the target type does not contain a default constructor.")]
        [MemberData(nameof(DefaultConstructorErrorMessageData))]
        public void DefaultConstructorErrorMessage(Action act, Type targetType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You specified that the DI container should use the default constructor of type \"{targetType}\", but this type contains no default constructor.");
        }

        public static TestData DefaultConstructorErrorMessageData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<B>().UseDefaultConstructor()), typeof(B) },
                new object[] { new Action(() => CreateRegistrationOptions<C>().UseDefaultConstructor()), typeof(C) }
            };

        [Theory(DisplayName = "UseConstructorWithParameter must throw a TypeRegistrationException when the target type does not contain the specified constructor.")]
        [MemberData(nameof(ConstructorWithOneParameterErrorMessageData))]
        public void ConstructorWithOneParameterErrorMessage(Action act, Type targetType, Type parameterType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"You specified that the DI container should use the constructor with a single parameter of type \"{parameterType}\", but type \"{targetType}\" does not contain such a constructor.");
        }

        public static TestData ConstructorWithOneParameterErrorMessageData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<A>().UseConstructorWithParameter<int>()), typeof(A), typeof(int) },
                new object[] { new Action(() => CreateRegistrationOptions<D>().UseConstructorWithParameter<A>()), typeof(D), typeof(A) }
            };

        [Theory(DisplayName = "UseConstructorWithParameters must throw a TypeRegistrationException when the target type does not contain the specified constructor.")]
        [MemberData(nameof(ConstructorWithSeveralParametersErrorMessageData))]
        public void ConstructorWithSeveralParametersErrorMessage(Action act, Type targetType, Type[] parameterTypes)
        {
            var exceptionMessage = new StringBuilder().Append("You specified that the DI container should use the constructor with the type parameters ")
                                                      .AppendWordEnumeration(parameterTypes)
                                                      .Append($", but type \"{targetType}\" does not contain such a constructor.")
                                                      .ToString();

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain(exceptionMessage);
        }

        public static TestData ConstructorWithSeveralParametersErrorMessageData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<A>().UseConstructorWithParameters<int, double, string>()), typeof(A), new[] { typeof(int), typeof(double), typeof(string) } },
                new object[] { new Action(() => CreateRegistrationOptions<E>().UseConstructorWithParameters<int, uint, string, DateTime, double, Guid>()), typeof(E), new[] { typeof(int), typeof(uint), typeof(string), typeof(DateTime), typeof(double), typeof(Guid) } },
                new object[] { new Action(() => CreateRegistrationOptions<E>().UseConstructorWithParameters<int, uint, string, DateTime, double, Guid, string>()), typeof(E), new[] { typeof(int), typeof(uint), typeof(string), typeof(DateTime), typeof(double), typeof(Guid), typeof(string) } },
                new object[] { new Action(() => CreateRegistrationOptions<E>().UseConstructorWithParameters<int, uint, string, DateTime, double, Guid, string, short>()), typeof(E), new[] { typeof(int), typeof(uint), typeof(string), typeof(DateTime), typeof(double), typeof(Guid), typeof(string), typeof(short) } }
            };

        [Fact(DisplayName = "UseStaticFactoryMethod must throw a TypeRegistrationException when the delegate does not point to a public static method returning an instance of the target type.")]
        public void StaticMethodWrongDelegate()
        {
            Action act = () => CreateRegistrationOptions<A>().UseStaticFactoryMethod(new Func<string>(() => "Foo"));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified delegate does not describe a public, static method that returns an instance of type {typeof(A)}.");
        }

        [Fact(DisplayName = "UseStaticFactoryMethod must throw a TypeRegistrationException when the expression does not point to a public static method returning an instance of the target type.")]
        public void StaticMethodWrongExpression()
        {
            Action act = () => CreateRegistrationOptions<A>().UseStaticFactoryMethod(() => StaticCreateA());

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"Your expression to select a static factory method for type {typeof(A)} does not describe a public static method. A valid example would be \"() => MyType.Create(default(string), default(Foo))\".");
        }

        private static A StaticCreateA()
        {
            return new A();
        }

        [Fact(DisplayName = "UseStaticFactoryMethod must throw a TypeRegistrationException when the methodInfo does not point to a public static method returning an instance of the target type.")]
        public void StaticMethodWrongMethodInfo()
        {
            Action act = () => CreateRegistrationOptions<A>().UseStaticFactoryMethod(GetType().GetRuntimeMethod("InstanceCreateA", new Type[0]));

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified method info does not describe a public, static method that returns an instance of type {typeof(A)}");
        }

        public A InstanceCreateA()
        {
            return new A();
        }

        private static RegistrationOptions<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptions<T>(new ConstructorWithMostParametersSelector(), new[] { typeof(IDisposable) });
        }
    }
}