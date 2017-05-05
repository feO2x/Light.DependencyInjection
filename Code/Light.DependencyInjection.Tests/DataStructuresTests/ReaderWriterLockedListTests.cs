using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

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

        [Fact]
        public void IncreaseCapacity()
        {
            var testTarget = new ReaderWriterLockedList<int>();
            var initialCapacity = testTarget.Capacity;

            for (var i = 0; i < initialCapacity * 2; i++)
            {
                testTarget.Add(i);
            }

            testTarget.Capacity.Should().BeGreaterThan(initialCapacity * 2);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 })]
        [InlineData(new[] { 42, -188, 481, 67, -55454, 12 })]
        [InlineData(new int[] { })]
        public void Clear(int[] initialItems)
        {
            var testTarget = new ReaderWriterLockedList<int>(initialItems);

            testTarget.Clear();

            testTarget.Should().BeEmpty();
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3, 4 }, 3, true)]
        [InlineData(new[] { 42, -13, 5 }, 6, false)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Foo", true)]
        [InlineData(new string[] { }, "Foo", false)]
        public void Contains<T>(T[] existingItems, T item, bool expected)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            var actual = testTarget.Contains(item);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 2, 1)]
        [InlineData(new[] { 42, 87, 1005 }, -4, -1)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Foo", 0)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Baz", 2)]
        [InlineData(new string[] { }, "Foo", -1)]
        public void IndexOf<T>(T[] existingItems, T item, int expectedIndex)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            var actualIndex = testTarget.IndexOf(item);

            actualIndex.Should().Be(expectedIndex);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 2, new[] { 1, 3 }, true)]
        [InlineData(new[] { 1, 2, 3 }, 5, new[] { 1, 2, 3 }, false)]
        [InlineData(new[] { "Foo", "Bar" }, "Foo", new[] { "Bar" }, true)]
        [InlineData(new string[] { }, "Foo", new string[] { }, false)]
        public void Remove<T>(T[] existingItems, T item, T[] expectedCollection, bool expectedReturnValue)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            var actualReturnValue = testTarget.Remove(item);

            actualReturnValue.Should().Be(expectedReturnValue);
            testTarget.Should().Equal(expectedCollection);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 2, new[] { 1, 2 })]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, 0, new[] { "Bar", "Baz" })]
        public void RemoveAt<T>(T[] existingItems, int index, T[] expectedCollection)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            testTarget.RemoveAt(index);

            testTarget.Should().Equal(expectedCollection);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 2, 3)]
        [InlineData(new[] { 1, 2, 3 }, 0, 1)]
        [InlineData(new[] { "Foo", "Bar" }, 1, "Bar")]
        public void Get<T>(T[] existingItems, int index, T expected)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            var actual = testTarget[index];

            actual.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(CopyToData))]
        public void CopyTo<T>(T[] existingItems, T[] array, int startIndex)
        {
            var testTarget = new ReaderWriterLockedList<T>(existingItems);

            testTarget.CopyTo(array, startIndex);

            array.Should().Contain(existingItems);
        }

        public static readonly TestData CopyToData =
            new[]
            {
                new object[] { new[] { "Foo", "Bar", "Baz" }, new string[3], 0 },
                new object[] { new[] { "Foo", "Bar", "Baz" }, new string[5], 2 },
                new object[] { new[] { 1, 2, 3, 4, 5 }, new int[10], 0 }
            };
    }
}