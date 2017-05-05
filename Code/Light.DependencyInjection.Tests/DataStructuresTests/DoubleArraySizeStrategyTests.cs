using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Light.GuardClauses;
using Xunit;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    [Trait("Category", "Functional Tests")]
    public sealed class DoubleArraySizeStrategyTests
    {
        [Fact]
        public void DefaultCapacityIsFour()
        {
            DoubleArraySizeStrategy<int>.DefaultCapacity.Should().Be(4);
        }

        [Fact]
        public void InitialArrayWithoutExistingItems()
        {
            var testTarget = new DoubleArraySizeStrategy<int>();

            var array = testTarget.CreateInitialArray();

            array.Length.Should().Be(testTarget.InitialCapacity);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 4)]
        [InlineData(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 8)]
        [InlineData(new[] { "Foo", "Bar", "Baz", "Qux", "Quux", "Corge", "Grault" }, 8)]
        public void InitialArrayWithExistingItems<T>(T[] existingItems, int expectedCapacity)
        {
            var testTarget = new DoubleArraySizeStrategy<T>();

            var array = testTarget.CreateInitialArray(existingItems);

            array.Length.Should().Be(expectedCapacity);
            array.MustStartWith(existingItems);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3, 4, 5, 6 })]
        [InlineData(new[] { 1, 2, 3 })]
        public void IncreaseArraySize<T>(T[] array)
        {
            var testTarget = new DoubleArraySizeStrategy<T>();

            var newArray = testTarget.CreateLargerArrayFrom(array);

            newArray.Length.Should().Be(array.Length * 2);
            newArray.MustStartWith(array);
        }
    }
}