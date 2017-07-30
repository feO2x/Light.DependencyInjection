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
        [Theory(DisplayName = "A type key can be initialized without a registration name. In this case, RegistrationName must be an empty string and the hash codes must be created from the passed in Type object.")]
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
            typeKey.HashCode.Should().Be(type.GetHashCode());
            typeKey.RegistrationName.Should().BeEmpty();
        }

        [Theory(DisplayName = "A type key can be initialized with a registration name. In this case, RegistrationName must be the passed in string and the hash code must be created from both type and registrationName.")]
        [InlineData(typeof(object), "Foo")]
        [InlineData(typeof(string), "Bar")]
        [InlineData(typeof(double), "Baz")]
        public void InitializationWithRegistrationName(Type type, string registrationName)
        {
            var typeKey = new TypeKey(type, registrationName);

            typeKey.RegistrationName.Should().BeSameAs(registrationName);
            typeKey.ToString().Should().Be($"\"{type}\" with name \"{registrationName}\"");
            typeKey.HashCode.Should().NotBe(type.GetHashCode());
        }

        [Fact(DisplayName = "The constructor of TypeKey must throw an ArgumentNullException when type is null.")]
        public void TypeNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new TypeKey(null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("type");
        }

        [Fact(DisplayName = "The constructor of TypeKey must throw an ArgumentNullException when registration name is null.")]
        public void RegistrationNameNull()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action act = () => new TypeKey(typeof(object), null);

            act.ShouldThrow<ArgumentNullException>()
               .And.ParamName.Should().Be("registrationName");
        }

        [Theory(DisplayName = "Type Keys with the same values (type and registration name) must be equal (Value Object).")]
        [InlineData(typeof(int), "Foo")]
        [InlineData(typeof(string), "")]
        public void TwoTypeKeyInstancesAreEqualWhenTypeAndRegistrationNameAreIdentical(Type type, string registrationName)
        {
            var first = new TypeKey(type, registrationName);
            var second = new TypeKey(type, registrationName);

            (first == second).Should().BeTrue();
            (first != second).Should().BeFalse();
        }

        [Theory(DisplayName = "Type Keys with different values (type and registration name) must not be equal (Value Object).")]
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

        [Theory(DisplayName = "Type objects can be implicitly converted to TypeKey instances.")]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(decimal))]
        public void ImplicitConversionToTypeKey(Type type)
        {
            TypeKey typeKey = type;

            CheckTypeKeyInvariantsAfterInitializationWithType(type, typeKey);
        }

        [Fact(DisplayName = "TypeKey instances can be implicitly converted to Type objects.")]
        public void ImplicitConversionToType()
        {
            var typeKey = new TypeKey(typeof(string), "Foo");

            Type type = typeKey;

            type.Should().BeSameAs(typeKey.Type);
        }

        [Theory(DisplayName = "The overridden GetHashCode method must return the HashCode computed on initialization.")]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(float), "Foo")]
        public void GetHashCodeReturnsHashCode(Type type, string registrationName)
        {
            var typeKey = new TypeKey(type, registrationName);

            typeKey.GetHashCode().Should().Be(typeKey.HashCode);
        }

        [Theory(DisplayName = "Equals(object) must behave correctly.")]
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
                new[] { new object(), false },
                new object[] { null, false }
            };

        [Theory(DisplayName = "IsEmpty must return true if the TypeKey instance was created via struct initializer (not the constructor).")]
        [MemberData(nameof(IsEmptyData))]
        public void IsEmpty(TypeKey typeKey, bool expected)
        {
            typeKey.IsEmpty.Should().Be(expected);
        }

        public static readonly TestData IsEmptyData =
            new[]
            {
                new object[] { new TypeKey(), true },
                new object[] { new TypeKey(typeof(string)), false },
                new object[] { new TypeKey(typeof(int), "Foo"), false }
            };

        [Fact(DisplayName = "MustNotBeEmpty must throw an ArgumentException when the TypeKey was not instantiated via the constructor.")]
        public void MustNotBeEmpty()
        {
            Action act = () => new TypeKey().MustNotBeEmpty("foo");

            act.ShouldThrow<ArgumentException>()
               .And.ParamName.Should().Be("foo");
        }
    }
}