using FluentAssertions;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Xunit;

namespace Light.DependencyInjection.Tests
{
    public sealed class ImmutableAvlNodeTests
    {
        private static readonly TypeKey FirstTypeKey = new TypeKey(typeof(A));
        private static readonly TypeKey SecondTypeKey = new TypeKey(typeof(B));
        private static readonly TypeKey ThirdTypeKey = new TypeKey(typeof(C));
        private static readonly TypeKey FouthTypeKey = new TypeKey(typeof(D));

        [Fact(DisplayName = "ImmutableAvlNode.Empty describes an empty node of an AVL tree.")]
        public void EmptyNode()
        {
            var testTarget = ImmutableAvlNode<object>.Empty;

            testTarget.IsEmpty.Should().BeTrue();
            testTarget.Height.Should().Be(0);
            testTarget.NodeCount.Should().Be(0);
            testTarget.EntryCount.Should().Be(0);
            testTarget.LeftChild.Should().BeNull();
            testTarget.RightChild.Should().BeNull();
            testTarget.Duplicates.Should().BeSameAs(ImmutableList<HashEntry<TypeKey, object>>.Empty);
            testTarget.HashEntry.Should().Be(default(HashEntry<TypeKey, object>));
        }

        [Fact(DisplayName = "Empty AVL nodes are replaced by a single node with a hash entry.")]
        public void SetSingleNode()
        {
            var empty = ImmutableAvlNode<object>.Empty;
            var hashEntry = new HashEntry<TypeKey, object>(42, new TypeKey(), new object());

            var testTarget = empty.Add(hashEntry);

            testTarget.HashEntry.Should().Be(hashEntry);
            testTarget.IsEmpty.Should().BeFalse();
            testTarget.Height.Should().Be(1);
            testTarget.NodeCount.Should().Be(1);
            testTarget.EntryCount.Should().Be(1);
            testTarget.LeftChild.Should().Be(empty);
            testTarget.RightChild.Should().Be(empty);
            testTarget.Duplicates.Should().BeSameAs(ImmutableList<HashEntry<TypeKey, object>>.Empty);
        }

        [Fact(DisplayName = "AVL nodes must be rotated left when a node is right-heavy.")]
        public void RotateLeft()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(42)
                                                   .Add(55)
                                                   .Add(66);

            rootNode.HashEntry.HashCode.Should().Be(55);
            rootNode.LeftChild.HashEntry.HashCode.Should().Be(42);
            rootNode.RightChild.HashEntry.HashCode.Should().Be(66);
        }

        [Fact(DisplayName = "AVL nodes must be rotated right when a node is left-heavy.")]
        public void RotateRight()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(42)
                                                   .Add(33)
                                                   .Add(21);

            rootNode.HashEntry.HashCode.Should().Be(33);
            rootNode.LeftChild.HashEntry.HashCode.Should().Be(21);
            rootNode.RightChild.HashEntry.HashCode.Should().Be(42);
        }

        [Fact(DisplayName = "AVL nodes must be rotated right and then left when a node is zig-zag right heavy.")]
        public void RotateRightLeft()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(50)
                                                   .Add(70)
                                                   .Add(60);

            rootNode.HashEntry.HashCode.Should().Be(60);
            rootNode.LeftChild.HashEntry.HashCode.Should().Be(50);
            rootNode.RightChild.HashEntry.HashCode.Should().Be(70);
        }

        [Fact(DisplayName = "AVL nodes must be rotated left and then right when a node is zig-zag left-heavy.")]
        public void RotateLeftRight()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(50)
                                                   .Add(30)
                                                   .Add(40);

            rootNode.HashEntry.HashCode.Should().Be(40);
            rootNode.LeftChild.HashEntry.HashCode.Should().Be(30);
            rootNode.RightChild.HashEntry.HashCode.Should().Be(50);
        }

        [Fact(DisplayName = "AVL nodes must store entries with the same hash code as duplicates.")]
        public void DuplicateHash()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(30, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(30, SecondTypeKey, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == SecondTypeKey);
            rootNode.HashEntry.HashCode.Should().Be(30);
            rootNode.LeftChild.Should().Be(ImmutableAvlNode<object>.Empty);
            rootNode.RightChild.Should().Be(ImmutableAvlNode<object>.Empty);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a child node is added.")]
        public void NewChildDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(90, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(90, SecondTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(110, ThirdTypeKey, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == SecondTypeKey);
            rootNode.RightChild.HashEntry.HashCode.Should().Be(110);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated left-right.")]
        public void RotateLeftRightDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(70, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(70, SecondTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(60, ThirdTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(65, FouthTypeKey, null));

            rootNode.RightChild.Duplicates.Should().ContainSingle(entry => entry.Key == SecondTypeKey);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated right-left.")]
        public void RotateRightLeftDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(99, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(99, SecondTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(110, ThirdTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(105, FouthTypeKey, null));

            rootNode.LeftChild.Duplicates.Should().ContainSingle(entry => entry.Key == SecondTypeKey);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated left.")]
        public void RotateLeftDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(100, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(100, SecondTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(105, ThirdTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(110, FouthTypeKey, null));

            rootNode.LeftChild.Duplicates.Should().ContainSingle(entry => entry.Key == SecondTypeKey);
        }

        [Fact(DisplayName = "An AVL node must remain its duplicate entries when a tree is rotated right.")]
        public void RotateRightDuplicatesRemain()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(new HashEntry<TypeKey, object>(30, FirstTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(25, SecondTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(25, ThirdTypeKey, null))
                                                   .Add(new HashEntry<TypeKey, object>(20, FouthTypeKey, null));

            rootNode.Duplicates.Should().ContainSingle(entry => entry.Key == ThirdTypeKey);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry on the top node.")]
        public void SimpleReplace()
        {
            var node = ImmutableAvlNode<object>.Empty
                                               .Add(42);
            var replacedValue = new object();

            var newNode = node.Replace(new HashEntry<TypeKey, object>(42, new TypeKey(), replacedValue));

            newNode.HashEntry.Value.Should().BeSameAs(replacedValue);
            newNode.LeftChild.Should().BeSameAs(ImmutableAvlNode<object>.Empty);
            newNode.RightChild.Should().BeSameAs(ImmutableAvlNode<object>.Empty);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry on a child node in the left subtree.")]
        public void ReplaceInLeftChild()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(52)
                                                   .Add(36)
                                                   .Add(66)
                                                   .Add(40)
                                                   .Add(22);
            var replacedValue = new object();

            var newRootNode = rootNode.Replace(new HashEntry<TypeKey, object>(22, new TypeKey(), replacedValue));

            newRootNode.LeftChild.LeftChild.HashEntry.Value.Should().BeSameAs(replacedValue);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry on a child node in the right subtree.")]
        public void ReplaceInRightChild()
        {
            var rootNode = ImmutableAvlNode<object>.Empty
                                                   .Add(40)
                                                   .Add(20)
                                                   .Add(80)
                                                   .Add(60)
                                                   .Add(100);
            var replacedValue = new object();

            var newRootNode = rootNode.Replace(new HashEntry<TypeKey, object>(80, new TypeKey(), replacedValue));

            newRootNode.RightChild.HashEntry.Value.Should().BeSameAs(replacedValue);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry in the duplicates collection of the top node.")]
        public void ReplaceDuplicate()
        {
            var node = ImmutableAvlNode<object>.Empty
                                               .Add(42)
                                               .Add(new HashEntry<TypeKey, object>(42, FirstTypeKey, null));
            var replacedValue = new object();

            var newNode = node.Replace(new HashEntry<TypeKey, object>(42, FirstTypeKey, replacedValue));

            newNode.Duplicates.Should().ContainSingle(entry => entry.Value == replacedValue);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry in the duplicates collection of a node in the left subtree.")]
        public void ReplaceDuplicateOnLeftChild()
        {
            var node = ImmutableAvlNode<object>.Empty
                                               .Add(42)
                                               .Add(32)
                                               .Add(new HashEntry<TypeKey, object>(32, FirstTypeKey, null));
            var replacedValue = new object();

            var newNode = node.Replace(new HashEntry<TypeKey, object>(32, FirstTypeKey, replacedValue));

            newNode.LeftChild.Duplicates.Should().ContainSingle(entry => entry.Value == replacedValue);
        }

        [Fact(DisplayName = "AVL trees must be able to replace an entry in the duplicates collection of a node in the right subtree.")]
        public void ReplaceDuplicateOnRightChild()
        {
            var node = ImmutableAvlNode<object>.Empty
                                               .Add(42)
                                               .Add(50)
                                               .Add(new HashEntry<TypeKey, object>(50, FirstTypeKey, null));
            var replacedValue = new object();

            var newNode = node.Replace(new HashEntry<TypeKey, object>(50, FirstTypeKey, replacedValue));

            newNode.RightChild.Duplicates.Should().ContainSingle(entry => entry.Value == replacedValue);
        }
    }

    public static class ImmutableAvlNodeTestExtensions
    {
        public static ImmutableAvlNode<object> Add(this ImmutableAvlNode<object> node, int nodeKey, object value = null)
        {
            return node.Add(new HashEntry<TypeKey, object>(nodeKey, new TypeKey(), value));
        }
    }
}