using System;
using FluentAssertions;
using Light.DependencyInjection.Registrations;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests.RegistrationsTests
{
    [Trait("Category", "Functional Tests")]
    public sealed class TypeKeyTests
    {
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        public void InitializationWithoutRegistrationName(Type type)
        {
            var typeKey = new TypeKey(type);

            CheckTypeKeyInvariantsAfterInitializationWithType(type, typeKey);
        }

        private static void CheckTypeKeyInvariantsAfterInitializationWithType(Type type, TypeKey typeKey)
        {
            typeKey.Type.Should().BeSameAs(type);
            typeKey.TypeHashCode.Should().Be(type.GetHashCode())
                   .And.Be(typeKey.HashCode);
            typeKey.RegistrationName.Should().BeEmpty();
            typeKey.FullRegistrationName.Should().Be($"\"{type}\"")
                   .And.Be(typeKey.ToString());
        }

        [Theory]
        [InlineData(typeof(object), "Foo")]
        [InlineData(typeof(string), "Bar")]
        [InlineData(typeof(double), "Baz")]
        public void InitializationWithRegistrationName(Type type, string registrationName)
        {
            var typeKey = new TypeKey(type, registrationName);

            typeKey.RegistrationName.Should().BeSameAs(registrationName);
            typeKey.FullRegistrationName.Should().Be($"\"{type}\" with name \"{registrationName}\"")
                   .And.Be(typeKey.ToString());
            typeKey.HashCode.Should().NotBe(type.GetHashCode());
        }

        [Fact]
        public void TypeNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new TypeKey(null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("type");
        }

        [Fact]
        public void RegistrationNameNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new TypeKey(typeof(object), null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("registrationName");
        }

        [Theory]
        [InlineData(typeof(int), "Foo")]
        [InlineData(typeof(string), "")]
        public void TwoTypeKeyInstancesAreEqualWhenTypeAndRegistrationNameAreIdentical(Type type, string registrationName)
        {
            var first = new TypeKey(type, registrationName);
            var second = new TypeKey(type, registrationName);

            (first == second).Should().BeTrue();
            (first != second).Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(DifferentTypeKeys))]
        public void DifferentTypeKeysAreNotEqual(TypeKey first, TypeKey second)
        {
            (first == second).Should().BeFalse();
            (first != second).Should().BeTrue();
        }

        public static readonly TestData DifferentTypeKeys =
            new[]
            {
                new object[] { new TypeKey(typeof(object)), new TypeKey(typeof(string)) },
                new object[] { new TypeKey(typeof(object), "Foo"), new TypeKey(typeof(object), "Bar") },
                new object[] { new TypeKey(typeof(int)), new TypeKey(typeof(int), "Baz") }
            };

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(decimal))]
        public void ImplicitConversionToTypeKey(Type type)
        {
            TypeKey typeKey = type;

            CheckTypeKeyInvariantsAfterInitializationWithType(type, typeKey);
        }

        [Fact]
        public void ImplicitConversionToType()
        {
            var typeKey = new TypeKey(typeof(string), "Foo");

            Type type = typeKey;

            type.Should().BeSameAs(typeKey.Type);
        }

        [Theory]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(float), "Foo")]
        public void GetHashCodeReturnsHashCode(Type type, string registrationName)
        {
            var typeKey = new TypeKey(type, registrationName);

            typeKey.GetHashCode().Should().Be(typeKey.HashCode);
        }

        [Theory]
        [MemberData(nameof(ObjectEqualsData))]
        public void ObjectEquals(object compareObject, bool expected)
        {
            var typeKey = new TypeKey(typeof(string), "Foo");

            var actual = typeKey.Equals(compareObject);

            actual.Should().Be(expected);
        }

        public static readonly TestData ObjectEqualsData =
            new[]
            {
                new object[] { new TypeKey(typeof(string), "Foo"), true },
                new object[] { new TypeKey(typeof(int)), false },
                new[] { new object(), false},
                new object[] { null, false}
               
            };
    }
}