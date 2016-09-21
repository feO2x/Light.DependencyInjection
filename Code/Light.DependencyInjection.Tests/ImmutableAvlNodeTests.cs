using FluentAssertions;
using Light.DependencyInjection.Multithreading;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ImmutableAvlNodeTests
    {
        [Fact(DisplayName = "ImmutableAvlNode.Empty describes an empty node of an AVL tree.")]
        public void EmptyNode()
        {
            var testTarget = ImmutableAvlNode<int, object>.Empty;

            testTarget.IsEmpty.Should().BeTrue();
            testTarget.Height.Should().Be(0);
            testTarget.NodeCount.Should().Be(0);
            testTarget.EntryCount.Should().Be(0);
            testTarget.LeftChild.Should().BeNull();
            testTarget.RightChild.Should().BeNull();
            testTarget.Duplicates.Should().BeSameAs(ImmutableList<HashEntry<int, object>>.Empty);
            testTarget.HashEntry.Should().Be(default(HashEntry<int, object>));
        }

        [Fact(DisplayName = "Empty AVL nodes are replaces by a single node with a value.")]
        public void SetSingleNode()
        {
            var empty = ImmutableAvlNode<int, object>.Empty;
            var hashEntry = new HashEntry<int, object>(42, 42, new object());

            var testTarget = empty.Add(hashEntry);

            testTarget.HashEntry.Should().Be(hashEntry);
            testTarget.IsEmpty.Should().BeFalse();
            testTarget.Height.Should().Be(1);
            testTarget.NodeCount.Should().Be(1);
            testTarget.EntryCount.Should().Be(1);
            testTarget.LeftChild.Should().Be(empty);
            testTarget.RightChild.Should().Be(empty);
            testTarget.Duplicates.Should().BeSameAs(ImmutableList<HashEntry<int, object>>.Empty);
        }

        [Fact(DisplayName = "AVL nodes must be rotated left when a node is right-heavy.")]
        public void RotateLeft()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(42)
                                                        .Add(55)
                                                        .Add(66);

            rootNode.HashEntry.Key.Should().Be(55);
            rootNode.LeftChild.HashEntry.Key.Should().Be(42);
            rootNode.RightChild.HashEntry.Key.Should().Be(66);
        }

        [Fact(DisplayName = "AVL nodes must be rotated right when a node is left-heavy.")]
        public void RotateRight()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(42)
                                                        .Add(33)
                                                        .Add(21);

            rootNode.HashEntry.Key.Should().Be(33);
            rootNode.LeftChild.HashEntry.Key.Should().Be(21);
            rootNode.RightChild.HashEntry.Key.Should().Be(42);
        }

        [Fact(DisplayName = "AVL nodes must be rotated right and then left when a node is zig-zag right heavy.")]
        public void RotateRightLeft()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(50)
                                                        .Add(70)
                                                        .Add(60);

            rootNode.HashEntry.Key.Should().Be(60);
            rootNode.LeftChild.HashEntry.Key.Should().Be(50);
            rootNode.RightChild.HashEntry.Key.Should().Be(70);
        }

        [Fact(DisplayName = "AVL nodes must be rotated left and then right when a node is zig-zag left-heavy.")]
        public void RotateLeftRight()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(50)
                                                        .Add(30)
                                                        .Add(40);

            rootNode.HashEntry.Key.Should().Be(40);
            rootNode.LeftChild.HashEntry.Key.Should().Be(30);
            rootNode.RightChild.HashEntry.Key.Should().Be(50);
        }

        [Fact(DisplayName = "AVL nodes must store entries with the same hash code as duplicates.")]
        public void DuplicateHash()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(30, 30, null))
                                                        .Add(new HashEntry<int, object>(30, 70, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == 70);
            rootNode.HashEntry.Key.Should().Be(30);
            rootNode.LeftChild.Should().Be(ImmutableAvlNode<int, object>.Empty);
            rootNode.RightChild.Should().Be(ImmutableAvlNode<int, object>.Empty);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a child node is added.")]
        public void NewChildDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(90, 90, null))
                                                        .Add(new HashEntry<int, object>(90, 100, null))
                                                        .Add(new HashEntry<int, object>(110, 110, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == 100);
            rootNode.RightChild.HashEntry.Key.Should().Be(110);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated left-right.")]
        public void RotateLeftRightDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(70, 70, null))
                                                        .Add(new HashEntry<int, object>(70, 80, null))
                                                        .Add(new HashEntry<int, object>(60, 60, null))
                                                        .Add(new HashEntry<int, object>(65, 65, null));

            rootNode.RightChild.Duplicates.Should().ContainSingle(entry => entry.Key == 80);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated right-left.")]
        public void RotateRightLeftDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(99, 99, null))
                                                        .Add(new HashEntry<int, object>(99, 101, null))
                                                        .Add(new HashEntry<int, object>(110, 110, null))
                                                        .Add(new HashEntry<int, object>(105, 105, null));

            rootNode.LeftChild.Duplicates.Should().ContainSingle(entry => entry.Key == 101);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated left.")]
        public void RotateLeftDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(100, 100, null))
                                                        .Add(new HashEntry<int, object>(100, 101, null))
                                                        .Add(new HashEntry<int, object>(105, 105, null))
                                                        .Add(new HashEntry<int, object>(110, 110, null));

            rootNode.LeftChild.Duplicates.Should().ContainSingle(entry => entry.Key == 101);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated right.")]
        public void RotateRightDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<int, object>.Empty
                                                        .Add(new HashEntry<int, object>(30, 30, null))
                                                        .Add(new HashEntry<int, object>(25, 25, null))
                                                        .Add(new HashEntry<int, object>(25, 26, null))
                                                        .Add(new HashEntry<int, object>(20, 20, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == 26);
        }
    }

    public static class ImmutableAvlNodeTestExtensions
    {
        public static ImmutableAvlNode<int, object> Add(this ImmutableAvlNode<int, object> node, int nodeKey, object value = null)
        {
            return node.Add(new HashEntry<int, object>(nodeKey, nodeKey, value));
        }
    }
}