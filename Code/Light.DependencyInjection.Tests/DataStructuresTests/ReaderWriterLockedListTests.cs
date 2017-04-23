using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Xunit;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ReaderWriterLockedListTests
    {
        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 4)]
        [InlineData(new[] { "Foo", "Bar" }, "Baz")]
        [InlineData(new string[] { }, "Foo")]
        public void Add<T>(T[] existingItems, T itemToAdd)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            testTarget.Add(itemToAdd);

            var expected = new List<T>(existingItems) { itemToAdd };
            testTarget.Should().Equal(expected);
        }

        [Fact]
        public void InitialCapacity()
        {
            var testTarget = new ReaderWriterLockedList<object>();

            testTarget.Capacity.Should().Be(ReaderWriterLockedList<object>.DefaultCapacity);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 10, new[] { 10, 1, 2, 3, 4, 5 })]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, 1, "Qux", new[] { "Foo", "Qux", "Bar", "Baz" })]
        [InlineData(new[] { "Foo", "Bar" }, "2", "Baz", new[] { "Foo", "Bar", "Baz" })]
        [InlineData(new int[] { }, 0, 42, new[] { 42 })]
        public void Insert<T>(T[] existingItems, int index, T item, T[] expected)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            testTarget.Insert(index, item);

            testTarget.Should().Equal(expected);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 2, 42, new[] { 1, 2, 42 })]
        [InlineData(new[] { "Foo", "Bar" }, 0, "Baz", new[] { "Baz", "Bar" })]
        public void Overwrite<T>(T[] existingItems, int index, T item, T[] expected)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            testTarget[index] = item;

            testTarget.Should().Equal(expected);
        }
    }
}