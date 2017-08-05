using System;
using System.Collections.Generic;
using FluentAssertions;
using Light.DependencyInjection.DataStructures;
using Light.GuardClauses;
using Xunit;
using TestData = System.Collections.Generic.IEnumerable<object[]>;

namespace Light.DependencyInjection.Tests.DataStructuresTests
{
    [Trait("Category", "Functional Tests")]
    public sealed class ReaderWriterLockedListTests
    {
        private readonly ReaderWriterLockSpy _lockSpy = new ReaderWriterLockSpy();

        private ReaderWriterLockedList<T> CreateTestTarget<T>(IEnumerable<T> existingItems = null)
        {
            return new ReaderWriterLockedList<T>(existingItems, @lock: _lockSpy);
        }

        [Theory(DisplayName = "Add must insert the items at the end of the list.")]
        [InlineData(new[] { 1, 2, 3 }, 4)]
        [InlineData(new[] { "Foo", "Bar" }, "Baz")]
        [InlineData(new string[] { }, "Foo")]
        public void Add<T>(T[] existingItems, T itemToAdd)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var testTarget = CreateTestTarget(existingItems);

            testTarget.Add(itemToAdd);

            var expected = new List<T>(existingItems) { itemToAdd };
            testTarget.Should().Equal(expected);
            testTarget.Count.Should().Be(existingItems.Length + 1);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Theory(DisplayName = "Insert must insert the items at the specified index.")]
        [InlineData(new[] { 1, 2, 3, 4, 5 }, 0, 10, new[] { 10, 1, 2, 3, 4, 5 })]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, 1, "Qux", new[] { "Foo", "Qux", "Bar", "Baz" })]
        [InlineData(new[] { "Foo", "Bar" }, "2", "Baz", new[] { "Foo", "Bar", "Baz" })]
        [InlineData(new int[] { }, 0, 42, new[] { 42 })]
        public void Insert<T>(T[] existingItems, int index, T item, T[] expected)
        {
            var testTarget = CreateTestTarget(existingItems);

            testTarget.Insert(index, item);

            testTarget.Should().Equal(expected);
            testTarget.Count.Should().Be(existingItems.Length + 1);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Theory(DisplayName = "The index property must overwrite the existing existing at the specified index with the given value.")]
        [InlineData(new[] { 1, 2, 3 }, 2, 42, new[] { 1, 2, 42 })]
        [InlineData(new[] { "Foo", "Bar" }, 0, "Baz", new[] { "Baz", "Bar" })]
        public void Overwrite<T>(T[] existingItems, int index, T item, T[] expected)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var testTarget = CreateTestTarget(existingItems);

            testTarget[index] = item;

            testTarget.Should().Equal(expected);
            testTarget.Count.Should().Be(existingItems.Length);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Fact(DisplayName = "The list must increase its capacity when there is no more space for new items.")]
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

        [Theory(DisplayName = "Clear must remove all items from the list.")]
        [InlineData(new[] { 1, 2, 3 })]
        [InlineData(new[] { 42, -188, 481, 67, -55454, 12 })]
        [InlineData(new int[] { })]
        public void Clear(int[] existingItems)
        {
            var testTarget = CreateTestTarget(existingItems);

            testTarget.Clear();

            testTarget.Should().BeEmpty();
            testTarget.Count.Should().Be(0);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Theory(DisplayName = "Contains must return true when the specified item is part of the list, else false.")]
        [InlineData(new[] { 1, 2, 3, 4 }, 3, true)]
        [InlineData(new[] { 42, -13, 5 }, 6, false)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Foo", true)]
        [InlineData(new string[] { }, "Foo", false)]
        public void Contains<T>(T[] existingItems, T item, bool expected)
        {
            var testTarget = CreateTestTarget(existingItems);

            var actual = testTarget.Contains(item);

            actual.Should().Be(expected);
            _lockSpy.MustHaveUsedReadLockExactlyOnce();
        }

        [Theory(DisplayName = "IndexOf must return the index of the target item if it is part of the list, or else -1.")]
        [InlineData(new[] { 1, 2, 3 }, 2, 1)]
        [InlineData(new[] { 42, 87, 1005 }, -4, -1)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Foo", 0)]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, "Baz", 2)]
        [InlineData(new string[] { }, "Foo", -1)]
        public void IndexOf<T>(T[] existingItems, T item, int expectedIndex)
        {
            var testTarget = CreateTestTarget(existingItems);

            var actualIndex = testTarget.IndexOf(item);

            actualIndex.Should().Be(expectedIndex);
            _lockSpy.MustHaveUsedReadLockExactlyOnce();
        }

        [Theory(DisplayName = "Remove must remove the specified item and return true if the item is part of the list, else it must return false.")]
        [InlineData(new[] { 1, 2, 3 }, 2, new[] { 1, 3 }, true)]
        [InlineData(new[] { 1, 2, 3 }, 5, new[] { 1, 2, 3 }, false)]
        [InlineData(new[] { "Foo", "Bar" }, "Foo", new[] { "Bar" }, true)]
        [InlineData(new string[] { }, "Foo", new string[] { }, false)]
        public void Remove<T>(T[] existingItems, T item, T[] expectedCollection, bool expectedReturnValue)
        {
            var testTarget = CreateTestTarget(existingItems);

            var wasRemoved = testTarget.Remove(item);

            wasRemoved.Should().Be(expectedReturnValue);
            testTarget.Should().Equal(expectedCollection);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();

            if (wasRemoved)
                testTarget.Count.Should().Be(existingItems.Length - 1);
        }

        [Theory(DisplayName = "RemoveAt must remove the item at the specified index.")]
        [InlineData(new[] { 1, 2, 3 }, 2, new[] { 1, 2 })]
        [InlineData(new[] { "Foo", "Bar", "Baz" }, 0, new[] { "Bar", "Baz" })]
        public void RemoveAt<T>(T[] existingItems, int index, T[] expectedCollection)
        {
            var testTarget = CreateTestTarget(existingItems);

            testTarget.RemoveAt(index);

            testTarget.Should().Equal(expectedCollection);
            testTarget.Count.Should().Be(existingItems.Length - 1);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Theory(DisplayName = "The property indexer must return an existing item if it is present.")]
        [InlineData(new[] { 1, 2, 3 }, 2, 3)]
        [InlineData(new[] { 1, 2, 3 }, 0, 1)]
        [InlineData(new[] { "Foo", "Bar" }, 1, "Bar")]
        public void Get<T>(T[] existingItems, int index, T expected)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var testTarget = CreateTestTarget(existingItems);

            var actual = testTarget[index];

            actual.Should().Be(expected);
            _lockSpy.MustHaveUsedReadLockExactlyOnce();
        }

        [Theory(DisplayName = "CopyTo must copy the all items to the target array, starting from the specified target index.")]
        [MemberData(nameof(CopyToData))]
        public void CopyTo<T>(T[] existingItems, T[] array, int startIndex)
        {
            var testTarget = CreateTestTarget(existingItems);

            testTarget.CopyTo(array, startIndex);

            array.Should().Contain(existingItems);
            _lockSpy.MustHaveUsedReadLockExactlyOnce();
        }

        public static readonly TestData CopyToData =
            new[]
            {
                new object[] { new[] { "Foo", "Bar", "Baz" }, new string[3], 0 },
                new object[] { new[] { "Foo", "Bar", "Baz" }, new string[5], 2 },
                new object[] { new[] { 1, 2, 3, 4, 5 }, new int[10], 0 }
            };

        [Fact(DisplayName = "IsReadonly must always return false.")]
        public void IsReadOnlyIsAlwaysFalse()
        {
            IList<object> testTarget = new ReaderWriterLockedList<object>();

            testTarget.IsReadOnly.Should().BeFalse();
        }

        [Fact(DisplayName = "ReaderWriterLockedList<T> must implement IConcurrentList<T>.")]
        public void ReaderWriterLockedListMustImplementIConcurrentList()
        {
            typeof(ReaderWriterLockedList<object>).Should().Implement<IConcurrentList<object>>();
        }

        [Fact(DisplayName = "GetOrAdd must return an existing item when the specified item is already in the list.")]
        public void ItemPresentOnGetOrAdd()
        {
            var guid = Guid.NewGuid();
            var firstDummy = new DummyEntity(guid, "A");
            var secondDummy = new DummyEntity(guid, "B");
            var testTarget = CreateTestTarget(new[] { firstDummy });

            var item = testTarget.GetOrAdd(secondDummy);

            testTarget.Count.Should().Be(1);
            item.Should().BeSameAs(firstDummy);
            _lockSpy.MustHaveUsedUpgradeableReadLockExactlyOnce()
                    .MustNotHaveUsedWriteLock();
        }

        [Fact(DisplayName = "GetOrAdd must add the specified item and return it when an equal item is not present in the list.")]
        public void ItemNotPresentOnGetOrAdd()
        {
            var firstDummy = new DummyEntity(Guid.NewGuid(), "A");
            var secondDummy = new DummyEntity(Guid.NewGuid(), "B");
            var testTarget = CreateTestTarget(new[] { firstDummy });

            var item = testTarget.GetOrAdd(secondDummy);

            testTarget.Count.Should().Be(2);
            item.Should().BeSameAs(secondDummy);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce()
                    .MustHaveUsedUpgradeableReadLockExactlyOnce();
        }

        [Fact(DisplayName = "AddOrUpdate must update the existing item when an equal item is present in the list.")]
        public void ItemPresentOnAddOrUpdate()
        {
            var guid = Guid.NewGuid();
            var firstDummy = new DummyEntity(guid, "A");
            var secondDummy = new DummyEntity(guid, "B");
            var testTarget = CreateTestTarget(new[] { firstDummy });

            testTarget.AddOrUpdate(secondDummy);

            testTarget.Count.Should().Be(1);
            testTarget[0].Should().BeSameAs(secondDummy);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        [Fact(DisplayName = "AddOrUpdate must add the specified item when an equal item is not present in the list.")]
        public void ItemNotPresentOnAddOrUpdate()
        {
            var firstDummy = new DummyEntity(Guid.NewGuid(), "A");
            var secondDummy = new DummyEntity(Guid.NewGuid(), "B");
            var testTarget = CreateTestTarget(new[] { firstDummy });

            testTarget.AddOrUpdate(secondDummy);

            testTarget.Count.Should().Be(2);
            testTarget[0].Should().BeSameAs(firstDummy);
            testTarget[1].Should().BeSameAs(secondDummy);
            _lockSpy.MustHaveUsedWriteLockExactlyOnce();
        }

        public sealed class DummyEntity : IEquatable<DummyEntity>
        {
            public readonly Guid Id;
            public readonly string SomeOtherValue;

            public DummyEntity(Guid id, string someOtherValue)
            {
                Id = id.MustNotBeEmpty();
                SomeOtherValue = someOtherValue.MustNotBeNullOrWhiteSpace();
            }

            public bool Equals(DummyEntity other)
            {
                if (ReferenceEquals(other, null))
                    return false;

                return other.Id == Id;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as DummyEntity);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(DummyEntity first, DummyEntity second)
            {
                if (ReferenceEquals(first, second))
                    return true;

                return !ReferenceEquals(first, null) && first.Equals(second);
            }

            public static bool operator !=(DummyEntity first, DummyEntity second)
            {
                return !(first == second);
            }
        }
    }
}