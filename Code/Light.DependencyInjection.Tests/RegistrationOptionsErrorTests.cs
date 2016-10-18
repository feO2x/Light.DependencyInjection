using System;
using System.Collections;
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
    public sealed class RegistrationOptionsErrorTests
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

        [Fact(DisplayName = "UseStaticFactoryMethod must throw a TypeRegistrationException when the methodInfo does not point to a public static method returning an instance of the target type.")]
        public void StaticMethodWrongMethodInfo()
        {
            var methodInfo = GetType().GetRuntimeMethod("InstanceCreateA", new Type[0]);

            Action act = () => CreateRegistrationOptions<A>().UseStaticFactoryMethod(methodInfo);

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified method info \"{methodInfo}\" does not describe a public, static method that returns an instance of type \"{typeof(A)}\".");
        }

        public A InstanceCreateA()
        {
            return new A();
        }

        [Theory(DisplayName = "AddPropertyInjection must throw a TypeRegistrationException when the PropertyInfo does not point to a public settable instance property.")]
        [MemberData(nameof(PropertyInfoExpressionErroneousData))]
        public void PropertyInfoExpressionErroneous(Action act, Type targetType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified expression does not describe a settable instance property of type \"{targetType}\". Please use an expression like the following one: \"o => o.Property\".");
        }

        public static readonly TestData PropertyInfoExpressionErroneousData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<H>().AddPropertyInjection(h => h.BooleanValue)), typeof(H) },
                new object[] { new Action(() => CreateRegistrationOptions<I>().AddPropertyInjection(i => i.Text)), typeof(I) },
                new object[] { new Action(() => CreateRegistrationOptions<I>().AddPropertyInjection(i => I.SomeStaticNumber)), typeof(I) },
                new object[] { new Action(() => CreateRegistrationOptions<ArrayList>().AddPropertyInjection(list => list[0])), typeof(ArrayList) }
            };

        [Theory(DisplayName = "AddPropertyInjection must throw a TypeRegistrationException when the PropertyInfo does not describe a property of the target type.")]
        [MemberData(nameof(PropertyInfoDoesNotBelongToTargetTypeData))]
        public void PropertyInfoDoesNotBelongToTargetType(Action act, Type targetType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The property info you provided does not belong to the target type \"{targetType}\".");
        }

        public static readonly TestData PropertyInfoDoesNotBelongToTargetTypeData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<A>().AddPropertyInjection(a => new G().ReferenceToA)), typeof(A) },
                new object[] { new Action(() => CreateRegistrationOptions<A>().AddPropertyInjection(typeof(G).GetRuntimeProperty("ReferenceToA"))), typeof(A) }
            };

        [Theory(DisplayName = "AddFieldInjection must throw a TypeRegistrationException when the FieldInfo does not point to a public instance field that is not read-only.")]
        [MemberData(nameof(FieldInfoExpressionErroneousData))]
        public void FieldInfoExpressionErroneous(Action act, Type targetType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified expression does not describe a settable instance field of type \"{targetType}\". Please use an expression like the following one: \"o => o.Field\".");
        }

        public static readonly TestData FieldInfoExpressionErroneousData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<F>().AddFieldInjection(f => f.Number)), typeof(F) },
                new object[] { new Action(() => CreateRegistrationOptions<H>().AddFieldInjection(h => H.StaticInstance)), typeof(H) }
            };

        [Theory(DisplayName = "AddFieldInjection must throw a TypeRegistrationException when the FieldInfo does not describe a field of the target type.")]
        [MemberData(nameof(FieldInfoExpressionDoesNotBelongToTargetTypeData))]
        public void FieldInfoExpressionDoesNotBelongToTargetType(Action act, Type targetType)
        {
            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The field info you provided does not belong to the target type \"{targetType}\".");
        }

        public static readonly TestData FieldInfoExpressionDoesNotBelongToTargetTypeData =
            new[]
            {
                new object[] { new Action(() => CreateRegistrationOptions<A>().AddFieldInjection(a => new H().BooleanValue)), typeof(A) },
                new object[] { new Action(() => CreateRegistrationOptions<A>().AddFieldInjection(typeof(H).GetRuntimeField("BooleanValue"))), typeof(A) }
            };

        [Fact(DisplayName = "ResolveInstantiationParameter must throw a TypeRegistrationException when there is no parameter with the specified type.")]
        public void ConfigureInstantiationParameterWithTypeNotPresent()
        {
            Action act = () => CreateRegistrationOptions<A>().ResolveInstantiationParameter<B>();

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified instantiation method for type \"{typeof(A)}\" does not have a parameter of type \"{typeof(B)}\".");
        }

        [Fact(DisplayName = "ResolveInstantiationParameter must throw a TypeRegistrationException when there is no parameter with the specified name.")]
        public void ConfigureInstantiationParameterWithNameNotPresent()
        {
            Action act = () => CreateRegistrationOptions<F>().InstantiateWith<string, int>(F.Create)
                                                             .ResolveInstantiationParameter("foo");

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified instantiation method for type \"{typeof(F)}\" does not have a parameter with name \"foo\".");
        }

        [Fact(DisplayName = "ResolveInstantiationParameter must throw a TypeRegistrationException when there are several parameters with the same type.")]
        public void ConfigureInstantiationParametersWithSameTypes()
        {
            Action act = () => CreateRegistrationOptions<K>().ResolveInstantiationParameter<string>();

            act.ShouldThrow<TypeRegistrationException>()
               .And.Message.Should().Contain($"The specified instantiation method for type \"{typeof(K)}\" has several parameters with type \"{typeof(string)}\". Please use the overload of \"ResolveInstantiationParameter\" where an additional parameter name can be specified.");
        }

        private static RegistrationOptionsForType<T> CreateRegistrationOptions<T>()
        {
            return new RegistrationOptionsForType<T>(new ConstructorWithMostParametersSelector(), new[] { typeof(IDisposable) });
        }
    }
}