using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Light.GuardClauses;
using Xunit;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    [Trait("Category", "Functional Tests")]
    public sealed class DoubleArraySizeStrategyTests
    {
        [Fact(DisplayName = "The default capacity of DoubleArraySizeStrategy is four.")]
        public void DefaultCapacityIsFour()
        {
            DoubleArraySizeStrategy<int>.DefaultCapacity.Should().Be(4);
        }

        [Fact(DisplayName = "An array that is created via DoubleArraySizeStrategy must have the default initial capacity.")]
        public void InitialArrayWithoutExistingItems()
        {
            var testTarget = new DoubleArraySizeStrategy<int>();

            var array = testTarget.CreateInitialArray();

            array.Length.Should().Be(testTarget.InitialCapacity);
        }

        [Theory(DisplayName = "DoubleArraySizeStrategy must increase the initial capacity when existing items exceed the default capacity.")]
        [InlineData(new[] { 1, 2, 3 }, 4)]
        [InlineData(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 8)]
        public void InitialArrayWithExistingItems(int[] existingItems, int expectedCapacity)
        {
            var testTarget = new DoubleArraySizeStrategy<int>();

            var array = testTarget.CreateInitialArray(existingItems);

            array.Length.Should().Be(expectedCapacity);
            array.MustStartWith(existingItems);
        }

        [Theory(DisplayName = "DoubleArraySizeStrategy must double the capacity when CreateLargerArrayFrom is called.")]
        [InlineData(new[] { 1, 2, 3, 4, 5, 6 })]
        [InlineData(new[] { 1, 2, 3 })]
        public void IncreaseArraySize(int[] array)
        {
            var testTarget = new DoubleArraySizeStrategy<int>();

            var newArray = testTarget.CreateLargerArrayFrom(array);

            newArray.Length.Should().Be(array.Length * 2);
            newArray.MustStartWith(array);
        }
    }
}