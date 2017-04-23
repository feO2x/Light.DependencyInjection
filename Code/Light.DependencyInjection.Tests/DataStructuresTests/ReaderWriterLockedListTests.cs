using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Xunit;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    [Trait("Category", "Functional Tests")]
    public class ReaderWriterLockedListTests
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
    }
}