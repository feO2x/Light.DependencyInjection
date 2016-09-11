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
    }

    public static class ImmutableAvlNodeTestExtensions
    {
        public static ImmutableAvlNode<int, object> Add(this ImmutableAvlNode<int, object> node, int nodeKey, object value = null)
        {
            return node.Add(new HashEntry<int, object>(nodeKey, nodeKey, value));
        }
    }
}